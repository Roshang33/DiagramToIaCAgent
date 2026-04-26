using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TerraformAgent.Infrastructure.RAG;

/// <summary>
/// Vector search backed by Pinecone (HTTP REST API).
/// - Use an IEmbeddingProvider to get the query embedding.
/// - Configure Pinecone endpoint and API key via PineconeOptions.
/// - Maps Pinecone match metadata to VectorSearchResult.
/// </summary>
public class VectorSearchService
{
    private readonly HttpClient _http;
    private readonly IEmbeddingProvider _embeddings;
    private readonly PineconeOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public VectorSearchService(HttpClient httpClient, IEmbeddingProvider embeddings, PineconeOptions options)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
        _options = options ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new ArgumentException("Pinecone API key is required", nameof(options));

        // Set auth header for all requests
        if (!_http.DefaultRequestHeaders.Contains("Api-Key"))
        {
            _http.DefaultRequestHeaders.Add("Api-Key", _options.ApiKey);
        }
        _http.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Search the configured Pinecone index for the supplied query text.
    /// </summary>
    public async Task<List<VectorSearchResult>> SearchAsync(string query, int topK = 5, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<VectorSearchResult>();

        // 1) Get embedding for query
        var vector = await _embeddings.EmbedAsync(query).ConfigureAwait(false);
        if (vector == null || vector.Length == 0)
            return new List<VectorSearchResult>();

        // 2) Build Pinecone query request
        var request = new PineconeQueryRequest
        {
            Vector = vector,
            TopK = topK,
            IncludeMetadata = true,
            IncludeValues = false,
            Namespace = _options.Namespace
        };

        var body = JsonSerializer.Serialize(request, _jsonOptions);
        var url = BuildQueryUrl();

        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var resp = await _http.PostAsync(url, content, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();

        var respStream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var pineResp = await JsonSerializer.DeserializeAsync<PineconeQueryResponse>(respStream, _jsonOptions, ct).ConfigureAwait(false);

        if (pineResp?.Matches == null)
            return new List<VectorSearchResult>();

        var results = new List<VectorSearchResult>(pineResp.Matches.Count);
        foreach (var m in pineResp.Matches)
        {
            // Map metadata keys if present; safe access using case-insensitive behavior of JsonSerializerDefaults.Web
            m.Metadata ??= new Dictionary<string, object>();

            results.Add(new VectorSearchResult
            {
                Id = m.Id,
                Score = m.Score,
                SourceUrl = GetMetadataString(m.Metadata, "source_url") ?? GetMetadataString(m.Metadata, "sourceUrl") ?? "",
                Repository = GetMetadataString(m.Metadata, "repository") ?? "",
                FilePath = GetMetadataString(m.Metadata, "file_path") ?? GetMetadataString(m.Metadata, "filePath") ?? "",
                Snippet = GetMetadataString(m.Metadata, "snippet") ?? ""
            });
        }

        return results;
    }

    private string GetMetadataString(Dictionary<string, object> meta, string key)
    {
        if (meta == null) return null;
        if (meta.TryGetValue(key, out var val) && val != null) return val.ToString();
        return null;
    }

    private string BuildQueryUrl()
    {
        // Expect _options.BaseUrl like "https://your-index-name-namespace.svc.<region>.pinecone.io"
        // Pinecone query endpoint is "[base]/query"
        var baseUrl = _options.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            // If user provided only index name and environment, build common host format:
            // https://{indexName}-{environment}.svc.pinecone.io
            if (string.IsNullOrWhiteSpace(_options.IndexName) || string.IsNullOrWhiteSpace(_options.Environment))
                throw new InvalidOperationException("Either BaseUrl or IndexName+Environment must be provided in PineconeOptions.");

            baseUrl = $"https://{_options.IndexName}-{_options.Environment}.svc.pinecone.io";
        }

        return $"{baseUrl}/query";
    }
}

/// <summary>
/// Options required to call Pinecone.3
/// Either set BaseUrl (full host) or IndexName + Environment to build the host automatically.
/// </summary>
public class PineconeOptions
{
    public string ApiKey { get; set; } = "";
    public string IndexName { get; set; } = "";
    public string Environment { get; set; } = ""; // e.g. "us-west1-gcp"
    public string BaseUrl { get; set; } = ""; // optional full URL to Pinecone index service
    public string Namespace { get; set; } = ""; // optional
}

/// <summary>
/// Representation of a document returned by search.
/// </summary>
public class VectorSearchResult
{
    public string Id { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string Repository { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string Snippet { get; set; } = "";
    public double Score { get; set; }
}

/// <summary>
/// Abstraction to produce embeddings. Plug in OpenAI/Azure/etc.
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// Return a vector embedding for the provided text. Use a consistent dimension for indexing and queries.
    /// </summary>
    Task<float[]> EmbedAsync(string text, int dimension = 1536);
}

/// <summary>
/// Minimal types to serialize/deserialize Pinecone /query payloads/responses.
/// Adjust as needed if Pinecone API updates.
/// </summary>
internal sealed class PineconeQueryRequest
{
    public float[] Vector { get; set; } = Array.Empty<float>();
    public int TopK { get; set; } = 5;
    public bool IncludeMetadata { get; set; } = true;
    public bool IncludeValues { get; set; } = false;
    public string Namespace { get; set; } = "";
}

internal sealed class PineconeQueryResponse
{
    public List<PineconeMatch>? Matches { get; set; }
    public object? Query { get; set; }
}

internal sealed class PineconeMatch
{
    public string Id { get; set; } = "";
    public double Score { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}