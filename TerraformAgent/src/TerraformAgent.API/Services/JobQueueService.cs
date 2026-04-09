using TerraformAgent.Core.Models;
using TerraformAgent.Persistence.Repositories;

namespace TerraformAgent.Api.Services;

public class JobQueueService
{
    private readonly IJobRepository _repository;

    public JobQueueService(IJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> EnqueueJobAsync(string prompt, string? diagramPath)
    {
        var job = new AgentJob
        {
            JobId = Guid.NewGuid().ToString(),
            Prompt = prompt,
            DiagramUrl = diagramPath,
            Status = "Queued",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateJobAsync(job);

        return job.JobId;
    }
}
