namespace TerraformAgent.Api.Models;

public class TerraformJobResponse
{
    public string JobId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? TerraformCode { get; set; }
}
