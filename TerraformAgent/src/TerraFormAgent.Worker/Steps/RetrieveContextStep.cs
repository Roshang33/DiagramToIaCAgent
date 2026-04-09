using Microsoft.Extensions.Logging;
using TerraformAgent.Core.Constants;
using TerraformAgent.Core.Models;
using TerraformAgent.Core.Interfaces; // IRetrievalPlanner
using TerraformAgent.Infrastructure.RAG; // IVectorSearchService (adjust namespace)

namespace TerraformAgent.Worker.Steps;

public sealed class RetrieveContextStep : IWorkflowStep
{
    private readonly IRagService _planner;
    private readonly IVectorSearchService _vectorSearch;
    private readonly ILogger<RetrieveContextStep> _logger;

    public string HandlesStatus => JobStatus.RetrievingContext;

    public RetrieveContextStep(
        IRagService planner,
        IVectorSearchService vectorSearch,
        ISourceLoader sourceLoader,
        ILogger<RetrieveContextStep> logger)
    {
        _planner = planner;
        _vectorSearch = vectorSearch;
        _sourceLoader = sourceLoader;
        _logger = logger;
    }

    public async Task ExecuteAsync(AgentJob job, CancellationToken ct)
    {
        _logger.LogInformation("Building retrieval queries for job {JobId}", job.JobId);

        if (job.Intent is null)
            throw new InvalidOperationException("Intent is missing for retrieval step");

        var queries = _planner.BuildQueries(job.Intent).ToList();
        var ragResults = new List<RagResult>();

        foreach (var q in queries)
        {
            var hits = await _vectorSearch.SearchAsync(q, topK: 5, ct);
            foreach (var hit in hits)
            {
                // Keep metadata only; content is loaded later by PromptComposer as needed
                ragResults.Add(new RagResult
                {
                    
                    FilePath = hit.FilePath,
                    Score = hit.Score,
                    Snippet = hit.Snippet,
                    
                });
            }
        }

        // Deduplicate and sort by score
        job.RagResults = ragResults
            .GroupBy(r => (r.Source, r.FilePath, r.Version))
            .Select(g => g.OrderByDescending(x => x.Score).First())
            .OrderByDescending(x => x.Score)
            .ToList();

        job.Status = JobStatus.GeneratingTerraform;
    }
}