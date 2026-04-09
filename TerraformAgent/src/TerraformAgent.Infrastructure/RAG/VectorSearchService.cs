namespace TerraformAgent.Infrastructure.RAG;

public class VectorSearchService
{
    public async Task<List<VectorSearchResult>> SearchAsync(string query)
    {
        await Task.Delay(50);

        return new List<VectorSearchResult>
        {
            new VectorSearchResult
            {
                SourceUrl = "https://github.com/example/terraform/vnet.tf",
                Repository = "infra-modules",
                FilePath = "network/vnet.tf",
                Score = 0.92
            }
        };
    }
}

public class VectorSearchResult
{
    public string SourceUrl { get; set; } = "";
    public string Repository { get; set; } = "";
    public string FilePath { get; set; } = "";
    public double Score { get; set; }
}