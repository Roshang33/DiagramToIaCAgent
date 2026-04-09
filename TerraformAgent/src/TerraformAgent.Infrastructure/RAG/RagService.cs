using TerraformAgent.Core.Interfaces;
using TerraformAgent.Core.Models;

namespace TerraformAgent.Infrastructure.RAG;

public class RagService : IRagService
{
    private readonly VectorSearchService _vectorSearch;

    public RagService(VectorSearchService vectorSearch)
    {
        _vectorSearch = vectorSearch;
    }

    public async Task<List<RagResult>> SearchAsync(string query)
    {
        var vectorResults = await _vectorSearch.SearchAsync(query);

        return vectorResults
            .Select(v => new RagResult
            {
                SourceUrl = v.SourceUrl,
                Repository = v.Repository,
                FilePath = v.FilePath,
                Score = v.Score
            })
            .ToList();
    }
}