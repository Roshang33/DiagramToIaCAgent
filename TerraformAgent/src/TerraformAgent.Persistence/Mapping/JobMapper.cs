using TerraformAgent.Core.Models;
using TerraformAgent.Persistence.Entities;

namespace TerraformAgent.Persistence.Mapping;

public static class JobMapper
{
    public static JobEntity ToEntity(AgentJob job)
    {
        return new JobEntity
        {
            RowKey = job.JobId,
            Prompt = job.Prompt ?? "",
            DiagramUrl = job.DiagramUrl ?? "",
            Status = job.Status,
            CreatedAt = job.CreatedAt,
            TerraformOutput = job.TerraformOutput ?? ""
        };
    }

    public static AgentJob ToModel(JobEntity entity)
    {
        return new AgentJob
        {
            JobId = entity.RowKey,
            Prompt = entity.Prompt,
            DiagramUrl = entity.DiagramUrl,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            TerraformOutput = entity.TerraformOutput
        };
    }
}