using Azure.Data.Tables;
using TerraformAgent.Core.Models;
using TerraformAgent.Persistence.Entities;
using TerraformAgent.Persistence.Mapping;

namespace TerraformAgent.Persistence.Repositories;

public class JobRepository : IJobRepository
{
    private readonly TableClient _table;

    public JobRepository(TableClient table)
    {
        _table = table;
    }

    public async Task<string> CreateJobAsync(AgentJob job)
    {
        var entity = JobMapper.ToEntity(job);

        await _table.AddEntityAsync(entity);

        return entity.RowKey;
    }

    public async Task<AgentJob?> GetJobAsync(string jobId)
    {
        try
        {
            var response = await _table.GetEntityAsync<JobEntity>("jobs", jobId);

            return JobMapper.ToModel(response.Value);
        }
        catch
        {
            return null;
        }
    }

    public async Task UpdateJobStatusAsync(string jobId, string status)
    {
        var entity = await _table.GetEntityAsync<JobEntity>("jobs", jobId);

        entity.Value.Status = status;

        await _table.UpdateEntityAsync(entity.Value, entity.Value.ETag);
    }

    public async Task SaveTerraformResultAsync(string jobId, string terraformCode)
    {
        var entity = await _table.GetEntityAsync<JobEntity>("jobs", jobId);

        entity.Value.TerraformOutput = terraformCode;
        entity.Value.CompletedAt = DateTime.UtcNow;
        entity.Value.Status = "Completed";

        await _table.UpdateEntityAsync(entity.Value, entity.Value.ETag);
    }

    public async Task<List<AgentJob>> GetPendingJobsAsync()
    {
        var results = new List<AgentJob>();

        await foreach (var entity in _table.QueryAsync<JobEntity>(x => x.Status == "Pending"))
        {
            results.Add(JobMapper.ToModel(entity));
        }

        return results;
    }
}