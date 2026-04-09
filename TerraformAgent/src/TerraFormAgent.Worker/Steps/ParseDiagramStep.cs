using System;
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

        var keywords = await _analyzer.ExtractInfrastructureKeywordsAsync(job.DiagramUrl);

        // Convert extracted keywords into a simple intent string for downstream steps.
        // (Downstream code expects `job.Intent` to be populated; adapt if you store structured data instead.)
        var intent = (keywords is not null && keywords.Count > 0)
            ? string.Join(", ", keywords)
            : string.Empty;

        // Attach extracted intent and advance state
        job.Intent = intent;
        job.Status = JobStatus.RetrievingContext;
    }
}