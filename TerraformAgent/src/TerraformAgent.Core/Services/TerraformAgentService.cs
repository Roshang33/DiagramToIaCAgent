using TerraformAgent.Core.Interfaces;
using TerraformAgent.Core.Models;

namespace TerraformAgent.Core.Services;

public class TerraformAgentService
{
    private readonly IDiagramAnalyzer _diagramAnalyzer;
    private readonly IRagService _ragService;
    private readonly ISourceLoader _sourceLoader;
    private readonly ITerraformGenerator _terraformGenerator;

    public TerraformAgentService(
        IDiagramAnalyzer diagramAnalyzer,
        IRagService ragService,
        ISourceLoader sourceLoader,
        ITerraformGenerator terraformGenerator)
    {
        _diagramAnalyzer = diagramAnalyzer;
        _ragService = ragService;
        _sourceLoader = sourceLoader;
        _terraformGenerator = terraformGenerator;
    }

    public async Task<TerraformResult> ProcessJobAsync(AgentJob job)
    {
        var keywords = new List<string>();

        if (!string.IsNullOrEmpty(job.DiagramUrl))
        {
            keywords = await _diagramAnalyzer
                .ExtractInfrastructureKeywordsAsync(job.DiagramUrl);
        }

        if (!string.IsNullOrEmpty(job.Prompt))
        {
            keywords.Add(job.Prompt);
        }

        var ragResults = new List<RagResult>();

        foreach (var keyword in keywords)
        {
            var results = await _ragService.SearchAsync(keyword);
            ragResults.AddRange(results);
        }

        var enrichedResults = new List<RagResult>();

        foreach (var result in ragResults.Take(5))
        {
            var content = await _sourceLoader.LoadFileContentAsync(result.SourceUrl);

            result.Snippet = content;

            enrichedResults.Add(result);
        }

        var terraform = await _terraformGenerator.GenerateAsync(
            job.Prompt ?? "",
            enrichedResults);

        return terraform;
    }
}