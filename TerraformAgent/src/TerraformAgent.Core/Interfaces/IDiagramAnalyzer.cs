namespace TerraformAgent.Core.Interfaces;

public interface IDiagramAnalyzer
{
    Task<List<string>> ExtractInfrastructureKeywordsAsync(string diagramUrl);
}