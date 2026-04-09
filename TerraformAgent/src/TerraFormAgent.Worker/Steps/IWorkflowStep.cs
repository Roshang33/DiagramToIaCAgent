using TerraformAgent.Core.Models;

namespace TerraformAgent.Worker.Steps;

/// <summary>
/// Contract for a single workflow step. Implementations should mutate the AgentJob
/// (update Intent, RagResults, GeneratedTerraform, Status, etc.) and set the next Status.
/// Steps MUST be resilient and idempotent where possible.
/// </summary>
public interface IWorkflowStep
{
    /// <summary>
    /// Which job status this step handles (string-based, use JobStatus constants).
    /// </summary>
    string HandlesStatus { get; }

    /// <summary>
    /// Execute the step. Update job.Status to the next expected status on success.
    /// </summary>
    Task ExecuteAsync(AgentJob job, CancellationToken ct);
}