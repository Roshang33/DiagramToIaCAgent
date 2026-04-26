namespace TerraformAgent.Core.Models;

public class AgentJob
{
    public string Error;

    public string JobId { get; set; } = Guid.NewGuid().ToString();

    public string? DiagramUrl { get; set; }

    public string? Prompt { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Dictionary<string, object> Metadata { get; set; } = new();

    public string? TerraformOutput { get; set; }
    public int AttemptCount { get; set; }
    public string Intent { get; set; }
    public List<RagResult> RagResults { get; set; }
}