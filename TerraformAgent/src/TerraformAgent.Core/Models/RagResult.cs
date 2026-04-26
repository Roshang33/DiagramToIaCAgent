namespace TerraformAgent.Core.Models;

public class RagResult
{
    public string SourceUrl { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string Repository { get; set; } = string.Empty;

    public string Snippet { get; set; } = string.Empty;

    public double Score { get; set; }
    public object Source { get; set; }
    public object Version { get; set; }
}