namespace TerraformAgent.Core.Models;

public class WorkflowState
{
    public string JobId { get; set; } = string.Empty;

    public string CurrentStep { get; set; } = "Initialized";

    public List<string> CompletedSteps { get; set; } = new();

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}