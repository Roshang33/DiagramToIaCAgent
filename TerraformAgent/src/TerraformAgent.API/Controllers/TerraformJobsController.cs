using Microsoft.AspNetCore.Mvc;
using TerraformAgent.Api.Models;
using TerraformAgent.Api.Services;
using TerraformAgent.Persistence.Repositories;

namespace TerraformAgent.Api.Controllers;

[ApiController]
[Route("api/terraform")]
public class TerraformJobsController : ControllerBase
{
    private readonly JobQueueService _queueService;
    private readonly FileStorageService _fileStorage;
    private readonly IJobRepository _repository;

    public TerraformJobsController(
        JobQueueService queueService,
        FileStorageService fileStorage,
        IJobRepository repository)
    {
        _queueService = queueService;
        _fileStorage = fileStorage;
        _repository = repository;
    }

    [HttpPost("generate")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Generate([FromForm] GenerateTerraformRequest request)
    {
        var diagramPath = await _fileStorage.SaveFileAsync(request.Diagram);

        var jobId = await _queueService.EnqueueJobAsync(
            request.Prompt,
            diagramPath);

        return Accepted(new
        {
            jobId
        });
    }

    [HttpGet("{jobId}")]
    public async Task<IActionResult> GetStatus(string jobId)
    {
        var job = await _repository.GetJobAsync(jobId);

        if (job == null)
            return NotFound();

        return Ok(new TerraformJobResponse
        {
            JobId = job.JobId,
            Status = job.Status,
            TerraformCode = job.TerraformCode
        });
    }
}
