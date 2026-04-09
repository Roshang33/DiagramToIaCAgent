namespace TerraformAgent.Core.Models;

public class TerraformResult
{
    public string TerraformCode { get; set; } = string.Empty;

    public List<string> ReferencedModules { get; set; } = new();

    public List<string> ValidationWarnings { get; set; } = new();

    public bool IsValid => ValidationWarnings.Count == 0;
}