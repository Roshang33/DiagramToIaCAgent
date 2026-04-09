namespace TerraformAgent.Contracts.Events;

public class TerraformJobCreatedEvent
{
    public string JobId { get; set; } = string.Empty;

    public string? DiagramUrl { get; set; }

    public string? Prompt { get; set; }

    public List<string>? Keywords { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}