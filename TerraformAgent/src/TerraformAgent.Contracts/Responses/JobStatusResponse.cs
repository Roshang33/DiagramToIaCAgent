namespace TerraformAgent.Contracts.Responses;

public class JobStatusResponse
{
    public string JobId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ResultUrl { get; set; }
}