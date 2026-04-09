using Microsoft.Extensions.Logging;
using TerraformAgent.Core.Models;
using TerraformAgent.Core.Constants;

namespace TerraformAgent.Worker.Steps;

public sealed class PackageArtifactsStep : IWorkflowStep
{
    private readonly IArtifactStore _artifactStore;
    private readonly IJobRepository _repo;
    private readonly ILogger<PackageArtifactsStep> _logger;

    public string HandlesStatus => JobStatus.Validating;

    public PackageArtifactsStep(
        IArtifactStore artifactStore,
        IJobRepository repo,
        ILogger<PackageArtifactsStep> logger)
    {
        _artifactStore = artifactStore;
        _repo = repo;
        _logger = logger;
    }

    public async Task ExecuteAsync(AgentJob job, CancellationToken ct)
    {
        _logger.LogInformation("Packaging artifacts for job {JobId}", job.JobId);

        if (string.IsNullOrEmpty(job.GeneratedTerraform))
            throw new InvalidOperationException("No generated terraform artifact URI available");

        var artifactUri = await _artifactStore.PersistGeneratedArtifactAsync(job.GeneratedTerraform, job.JobId, ct);

        job.OutputArtifactUri = artifactUri;
        job.Status = JobStatus.Completed;
    }
}   