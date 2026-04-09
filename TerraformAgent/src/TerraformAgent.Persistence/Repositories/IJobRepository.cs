using TerraformAgent.Core.Models;

namespace TerraformAgent.Persistence.Repositories;

public interface IJobRepository
{
    Task<string> CreateJobAsync(AgentJob job);

    Task<AgentJob?> GetJobAsync(string jobId);

    Task UpdateJobStatusAsync(string jobId, string status);

    Task SaveTerraformResultAsync(string jobId, string terraformCode);

    Task<List<AgentJob>> GetPendingJobsAsync();
    Task SaveAsync(AgentJob job);
}