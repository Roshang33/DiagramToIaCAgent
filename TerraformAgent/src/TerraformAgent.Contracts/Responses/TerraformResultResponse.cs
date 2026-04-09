namespace TerraformAgent.Contracts.Responses;

public class TerraformResultResponse
{
    public string JobId { get; set; } = string.Empty;

    public string TerraformCode { get; set; } = string.Empty;

    public List<string> ReferencedModules { get; set; } = new();

    public List<string> Warnings { get; set; } = new();

    public bool Success { get; set; }
}