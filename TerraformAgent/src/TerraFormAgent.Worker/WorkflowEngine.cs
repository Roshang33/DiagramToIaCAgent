
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TerraformAgent.Core.Constants;
using TerraformAgent.Core.Models;
using TerraformAgent.Persistence.Repositories;
using TerraformAgent.Worker.Options;
using TerraformAgent.Worker.Steps;


namespace TerraformAgent.Worker;

public sealed class WorkflowEngine
{
    private readonly ILogger<WorkflowEngine> _logger;
    private readonly IDictionary<string, IWorkflowStep> _stepMap;
    private readonly IJobRepository _repo;
    private readonly WorkflowOptions _options;

    public WorkflowEngine(
        IEnumerable<IWorkflowStep> steps,
        IJobRepository repo,
        IOptions<WorkflowOptions> options,
        ILogger<WorkflowEngine> logger)
    {
        _logger = logger;
        _repo = repo;
        _options = options.Value;

        // Map steps by the status they handle (string keys)
        _stepMap = steps.ToDictionary(s => s.HandlesStatus);
    }

    public async Task RunAsync(AgentJob job, CancellationToken ct)
    {
        if (job is null) throw new ArgumentNullException(nameof(job));

        _logger.LogInformation("Starting workflow for job {JobId} in state {State}", job.JobId, job.Status);

        // Keep processing until a terminal state
        while (!IsTerminal(job.Status))
        {
            ct.ThrowIfCancellationRequested();

            if (!_stepMap.TryGetValue(job.Status, out var step))
            {
                _logger.LogError("No workflow step registered for state {State}", job.Status);
                job.Status = JobStatus.Failed;
                job.Error = $"No handler for state {job.Status}";
                await _repo.SaveAsync(job);
                break;
            }

            try
            {
                _logger.LogInformation("Executing step {Step} for job {JobId}", step.GetType().Name, job.JobId);

                await step.ExecuteAsync(job, ct);

                // reset attempt count on success and persist
                job.AttemptCount = 0;
                await _repo.SaveAsync(job);
            }
            catch (Exception ex)
            {
                job.AttemptCount++;
                job.Error = ex.Message;
                _logger.LogWarning(ex, "Step {Step} failed for job {JobId} (attempt {Attempt})", step.GetType().Name, job.JobId, job.AttemptCount);

                if (job.AttemptCount >= _options.MaxRetriesPerStep)
                {
                    job.Status = JobStatus.Failed;
                    job.Error = $"Step {step.GetType().Name} failed after {job.AttemptCount} attempts. Last error: {ex.Message}";
                    await _repo.SaveAsync(job);
                    _logger.LogError("Job {JobId} failed permanently", job.JobId);
                    break;
                }

                // Backoff before retrying
                var delay = BackoffDelay(job.AttemptCount);
                _logger.LogInformation("Delaying {Delay}ms before retry for job {JobId}", delay.TotalMilliseconds, job.JobId);
                await Task.Delay(delay, ct);

                // persist attempt count and retry
                await _repo.SaveAsync(job);
            }
        }

        _logger.LogInformation("Workflow for job {JobId} completed with state {State}", job.JobId, job.Status);
    }

    private static bool IsTerminal(string status) =>
        status == JobStatus.Completed || status == JobStatus.Failed;

    private TimeSpan BackoffDelay(int attempt) =>
        TimeSpan.FromMilliseconds(Math.Min(_options.MaxBackoffMs, _options.BaseBackoffMs * Math.Pow(2, attempt - 1)));
}