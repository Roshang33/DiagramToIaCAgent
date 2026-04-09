using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TerraformAgent.Core.Models;
using TerraformAgent.Persistence.Repositories;

namespace TerraformAgent.Worker;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IJobQueue _jobQueue;
    private readonly IJobRepository _jobRepository;
    private readonly WorkflowEngine _engine;

    public Worker(
        ILogger<Worker> logger,
        IJobQueue jobQueue,
        IJobRepository jobRepository,
        WorkflowEngine engine)
    {
        _logger = logger;
        _jobQueue = jobQueue;
        _jobRepository = jobRepository;
        _engine = engine;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TerraformAgent Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Dequeue a job (returns null if none available)
                var jobEnvelope = await _jobQueue.DequeueAsync(stoppingToken);
                if (jobEnvelope is null)
                {
                    // No job: short delay and continue
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                _logger.LogInformation("Dequeued job {JobId}", jobEnvelope.JobId);

                // Persist initial job record if not present
                var job = await _jobRepository.GetAsync(jobEnvelope.JobId);
                if (job is null)
                {
                    job = new AgentJob
                    {
                        JobId = jobEnvelope.JobId,
                        TenantId = jobEnvelope.TenantId,
                        Request = jobEnvelope.Request,
                        Status = AgentJobStatus.Pending,
                        CreatedAtUtc = DateTimeOffset.UtcNow,
                        AttemptCount = 0
                    };

                    await _jobRepository.SaveAsync(job);
                }

                // Run the workflow engine for this job (will checkpoint state)
                await _engine.RunAsync(job, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutdown requested - break loop
                break;
            }
            catch (Exception ex)
            {
                // Top-level error handling: log and continue
                _logger.LogError(ex, "Unhandled exception in Worker loop");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("TerraformAgent Worker stopped.");
    }
}