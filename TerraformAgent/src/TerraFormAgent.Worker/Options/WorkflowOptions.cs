namespace TerraformAgent.Worker.Options;

public sealed class WorkflowOptions
{
    public int MaxRetriesPerStep { get; set; } = 3;
    public int BaseBackoffMs { get; set; } = 500;
    public int MaxBackoffMs { get; set; } = 30_000;
}