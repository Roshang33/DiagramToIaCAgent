using Microsoft.Extensions.Logging;
using TerraformAgent.Core.Models;
using TerraformAgent.Persistence;
using TerraformAgent.Infrastructure;
using TerraformAgent.Core.Constants;

namespace TerraformAgent.Worker.Steps;

public sealed class GenerateTerraformStep : IWorkflowStep
{
    private readonly IPromptComposer _composer;
    private readonly ITerraformGenerator _generator;
    private readonly ILogger<GenerateTerraformStep> _logger;

    public string HandlesStatus => JobStatus.GeneratingTerraform;

    public GenerateTerraformStep(
        IPromptComposer composer,
        ITerraformGenerator generator,
        ILogger<GenerateTerraformStep> logger)
    {
        _composer = composer;
        _generator = generator;
        _logger = logger;
    }

    public async Task ExecuteAsync(AgentJob job, CancellationToken ct)
    {
        _logger.LogInformation("Composing prompt for job {JobId}", job.JobId);

        if (job.Intent is null)
            throw new InvalidOperationException("Intent missing for Terraform generation");

        // Compose a token-budget-aware prompt (the composer will load snippets via SourceLoader)
        var prompt = await _composer.ComposeAsync(job.Intent, job.RagResults!, ct);

        _logger.LogInformation("Invoking Terraform generator for job {JobId}", job.JobId);

        var result = await _generator.GenerateAsync(prompt, ct);

        // result contains artifact references or raw files depending on implementation
        job.GeneratedTerraform = result.GeneratedZipUri;
        job.Status = JobStatus.Validating;
    }
}