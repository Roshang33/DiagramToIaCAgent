using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
// using TerraformAgent.Core.Agents; // IDiagramInterpreter (removed)
using TerraformAgent.Core.Models;
using TerraformAgent.Core.Constants;
using TerraformAgent.Core.Interfaces;

namespace TerraformAgent.Worker.Steps;

public sealed class ParseDiagramStep : IWorkflowStep
{
    private readonly IDiagramAnalyzer _analyzer;
    private readonly ILogger<ParseDiagramStep> _logger;

    public string HandlesStatus => JobStatus.Pending; // initial state

    public ParseDiagramStep(IDiagramAnalyzer analyzer, ILogger<ParseDiagramStep> logger)
    {
        _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(AgentJob job, CancellationToken ct)
    {
        _logger.LogInformation("Parsing diagram for job {JobId}", job.JobId);

        if (string.IsNullOrEmpty(job.DiagramUrl))
        {
            throw new InvalidOperationException("No diagram URI provided in job");
        }

        DiagramAnalysisResult? analysisResult = null;
        // Try to call AnalyzeAsync(string, CancellationToken) if the concrete analyzer exposes it.
        var analyzeMethod = _analyzer.GetType()
            .GetMethod("AnalyzeAsync", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy,
                       binder: null,
                       types: new[] { typeof(string), typeof(CancellationToken) },
                       modifiers: null);

        if (analyzeMethod is not null)
        {
            // Invoke AnalyzeAsync and await the returned Task<T>
            var taskObj = (Task)analyzeMethod.Invoke(_analyzer, new object[] { job.DiagramUrl, ct })!;
            await taskObj.ConfigureAwait(false);

            // Extract the Result property from Task<T>
            var resultProp = taskObj.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);
            if (resultProp is not null)
            {
                analysisResult = resultProp.GetValue(taskObj) as DiagramAnalysisResult;
            }
        }

        // Fallback: use the existing keywords method and synthesize a minimal analysis result
        if (analysisResult is null)
        {
            var keywords = await _analyzer.ExtractInfrastructureKeywordsAsync(job.DiagramUrl);
            var nodes = keywords?
                .Select(k => new DiagramNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = k,
                    Label = k,
                    Confidence = 1.0,
                    Bbox = new int[4]
                })
                .ToList() ?? new List<DiagramNode>();

            analysisResult = new DiagramAnalysisResult
            {
                Nodes = nodes,
                Edges = new List<DiagramEdge>()
            };
        }

        // Persist edges into job metadata so downstream steps can read relationships.
        job.Metadata["diagramEdges"] = analysisResult.Edges;

        // Build the intent from canonical node types (or fall back to empty)
        var intent = analysisResult.Nodes != null && analysisResult.Nodes.Count > 0
            ? string.Join(", ", analysisResult.Nodes.Select(n => n.Type).Distinct())
            : string.Empty;

        job.Intent = intent;
        job.Status = JobStatus.RetrievingContext;
    }
}