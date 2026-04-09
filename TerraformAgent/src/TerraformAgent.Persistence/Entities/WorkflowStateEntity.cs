using Azure;
using Azure.Data.Tables;

namespace TerraformAgent.Persistence.Entities;

public class WorkflowStateEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "workflow";

    public string RowKey { get; set; } = "";

    public string CurrentStep { get; set; } = "";

    public string CompletedSteps { get; set; } = "";

    public DateTime UpdatedAt { get; set; }

    public ETag ETag { get; set; }

    public DateTimeOffset? Timestamp { get; set; }
}