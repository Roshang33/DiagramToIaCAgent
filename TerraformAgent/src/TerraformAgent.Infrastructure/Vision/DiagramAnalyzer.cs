using TerraformAgent.Core.Interfaces;

namespace TerraformAgent.Infrastructure.Vision;

public class DiagramAnalyzer : IDiagramAnalyzer
{
    public async Task<List<string>> ExtractInfrastructureKeywordsAsync(string diagramUrl)
    {
        await Task.Delay(100);

        return new List<string>
        {
            "azure vnet",
            "subnet",
            "aks cluster",
            "load balancer"
        };
    }
}