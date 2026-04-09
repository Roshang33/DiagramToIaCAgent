namespace TerraformAgent.Core.Interfaces;

public interface ISourceLoader
{
    Task<string> LoadFileContentAsync(string sourceUrl);
}