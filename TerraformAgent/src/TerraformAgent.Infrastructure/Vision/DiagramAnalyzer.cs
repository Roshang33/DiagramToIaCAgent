
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TerraformAgent.Core.Interfaces;
using TerraformAgent.Core.Models;

namespace TerraformAgent.Infrastructure.Vision;

public class DiagramAnalyzer : IDiagramAnalyzer
{
    private readonly IOcrService _ocr;
    private readonly IIconDetector _iconDetector;
    private readonly ITopologyExtractor _topology;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<DiagramAnalyzer> _logger;

    // Simple normalization map: detector types and OCR keywords -> canonical names
    private static readonly Dictionary<string, string> _canonicalMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // icon types
        ["vnet_icon"] = "azure_vnet",
        ["subnet_icon"] = "subnet",
        ["aks_icon"] = "aks",
        ["lb_icon"] = "load_balancer",
        ["app_service_icon"] = "app_service",
        ["sql_icon"] = "azure_sql",
        ["storage_icon"] = "storage_blob",

        // common OCR terms that may appear
        ["azure vnet"] = "azure_vnet",
        ["vnet"] = "azure_vnet",
        ["subnet"] = "subnet",
        ["aks"] = "aks",
        ["kubernetes"] = "aks",
        ["load balancer"] = "load_balancer",
        ["app service"] = "app_service",
        ["sql"] = "azure_sql",
        ["storage"] = "storage_blob",
        ["blob"] = "storage_blob"
    };

    public DiagramAnalyzer(
        IOcrService ocr,
        IIconDetector iconDetector,
        ITopologyExtractor topology,
        IHttpClientFactory httpFactory,
        ILogger<DiagramAnalyzer> logger)
    {
        _ocr = ocr ?? throw new ArgumentNullException(nameof(ocr));
        _iconDetector = iconDetector ?? throw new ArgumentNullException(nameof(iconDetector));
        _topology = topology ?? throw new ArgumentNullException(nameof(topology));
        _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Backwards-compatible method required by Core interface.
    public async Task<List<string>> ExtractInfrastructureKeywordsAsync(string diagramUrl)
    {
        var result = await AnalyzeAsync(diagramUrl, CancellationToken.None);
        // return unique canonical keyword names
        return result.Nodes
            .Select(n => NormalizeToCanonical(n.Type, n.Label))
            .Where(k => !string.IsNullOrEmpty(k))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // New orchestrator method that returns structured graph
    public async Task<DiagramAnalysisResult> AnalyzeAsync(string diagramUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(diagramUrl))
            throw new ArgumentException("diagramUrl is required", nameof(diagramUrl));

        _logger.LogInformation("Downloading diagram from {Url}", diagramUrl);

        var imageBytes = await DownloadImageAsync(diagramUrl, ct);

        _logger.LogInformation("Running OCR");
        var ocrTokens = await _ocr.ExtractTextAsync(imageBytes, ct);

        _logger.LogInformation("Detecting icons");
        var icons = await _iconDetector.DetectIconsAsync(imageBytes, ct);

        _logger.LogInformation("Extracting topology (connectors)");
        var edgesDetected = await _topology.ExtractEdgesAsync(imageBytes, icons, ct);

        // Fusion: map detected icons to nodes and assign nearest OCR label if any
        var nodes = new List<DiagramNode>();
        for (var i = 0; i < icons.Count; i++)
        {
            var icon = icons[i];
            var nearestText = FindNearestOcrText(icon.Bbox, ocrTokens);
            var label = nearestText?.Text ?? string.Empty;

            nodes.Add(new DiagramNode
            {
                Id = Guid.NewGuid().ToString(),
                Type = icon.Type,
                Label = label,
                Confidence = icon.Confidence,
                Bbox = icon.Bbox
            });
        }

        // Build edges mapping icon index -> node id
        var edges = new List<DiagramEdge>();
        foreach (var e in edgesDetected)
        {
            if (e.FromIconIndex < 0 || e.FromIconIndex >= icons.Count) continue;
            if (e.ToIconIndex < 0 || e.ToIconIndex >= icons.Count) continue;

            var fromId = nodes[e.FromIconIndex].Id;
            var toId = nodes[e.ToIconIndex].Id;

            edges.Add(new DiagramEdge
            {
                FromNodeId = fromId,
                ToNodeId = toId,
                Type = e.Type,
                Confidence = e.Confidence
            });
        }

        // Normalization step: convert detector types / labels into canonical resource types
        foreach (var n in nodes)
        {
            var canonical = NormalizeToCanonical(n.Type, n.Label);
            if (!string.IsNullOrEmpty(canonical))
            {
                // store canonical in Type to make downstream simpler
                n.Type = canonical;
            }
        }

        var result = new DiagramAnalysisResult
        {
            Nodes = nodes,
            Edges = edges
        };

        return result;
    }

    private static (string Text, int[] Bbox, double Confidence)? FindNearestOcrText(int[] iconBbox, List<(string Text, int[] Bbox, double Confidence)> ocrTokens)
    {
        if (ocrTokens is null || ocrTokens.Count == 0) return null;

        // compute center of icon
        var ix = iconBbox[0] + iconBbox[2] / 2.0;
        var iy = iconBbox[1] + iconBbox[3] / 2.0;

        double bestDist = double.MaxValue;
        (string Text, int[] Bbox, double Confidence)? best = null;

        foreach (var t in ocrTokens)
        {
            var tb = t.Bbox;
            var tx = tb[0] + tb[2] / 2.0;
            var ty = tb[1] + tb[3] / 2.0;
            var dx = tx - ix;
            var dy = ty - iy;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = t;
            }
        }

        // heuristic: only accept if within a reasonable pixel distance (tune per image scale)
        if (bestDist <= 300) // arbitrary default; tune for your diagrams
            return best;

        return null;
    }

    private static string NormalizeToCanonical(string detectorTypeOrIcon, string ocrLabel)
    {
        // Try direct map from detector type first
        if (!string.IsNullOrWhiteSpace(detectorTypeOrIcon) && _canonicalMap.TryGetValue(detectorTypeOrIcon, out var mapped1))
            return mapped1;

        // Then try OCR label tokens
        if (!string.IsNullOrWhiteSpace(ocrLabel))
        {
            var toks = ocrLabel.Split(new[] { ' ', '/', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in toks)
            {
                if (_canonicalMap.TryGetValue(t.Trim(), out var mapped))
                    return mapped;
            }

            // Also try the full OCR label
            if (_canonicalMap.TryGetValue(ocrLabel.Trim(), out var mappedFull))
                return mappedFull;
        }

        // fallback: lower-cased detectorTypeOrIcon (sanitized)
        if (!string.IsNullOrWhiteSpace(detectorTypeOrIcon))
            return detectorTypeOrIcon.Replace(" ", "_").ToLowerInvariant();

        return string.Empty;
    }

    private async Task<byte[]> DownloadImageAsync(string diagramUrl, CancellationToken ct)
    {
        using var client = _httpFactory.CreateClient();
        using var resp = await client.GetAsync(diagramUrl, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsByteArrayAsync(ct);
    }
}