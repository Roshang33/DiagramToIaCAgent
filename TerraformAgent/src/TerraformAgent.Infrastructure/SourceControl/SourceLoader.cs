using TerraformAgent.Core.Interfaces;

namespace TerraformAgent.Infrastructure.SourceControl;

public class SourceLoader : ISourceLoader
{
    private readonly HttpClient _httpClient;

    public SourceLoader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> LoadFileContentAsync(string sourceUrl)
    {
        try
        {
            return await _httpClient.GetStringAsync(sourceUrl);
        }
        catch
        {
            return "";
        }
    }
}