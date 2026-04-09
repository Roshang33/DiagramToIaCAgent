using Azure;
using Azure.Data.Tables;

namespace TerraformAgent.Persistence.Entities;

public class JobEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "jobs";

    public string RowKey { get; set; } = Guid.NewGuid().ToString();

    public string Prompt { get; set; } = "";

    public string DiagramUrl { get; set; } = "";

    public string Status { get; set; } = "Pending";

    public string TerraformOutput { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public ETag ETag { get; set; }

    public DateTimeOffset? Timestamp { get; set; }
}