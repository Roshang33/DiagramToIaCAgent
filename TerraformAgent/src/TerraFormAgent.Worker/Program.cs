using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TerraformAgent.Worker;
using TerraformAgent.Worker.Steps;
using TerraformAgent.Worker.Options;

// NOTE: add project references to TerraformAgent.Core, TerraformAgent.Infrastructure, TerraformAgent.Persistence
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Options
        services.Configure<WorkflowOptions>(context.Configuration.GetSection("Workflow"));

        // Core & Infra services (assumed registered in other projects).
        // If those are not registered elsewhere, add their registrations here.
        // Example placeholders (replace with actual implementations):
        // services.AddSingleton<IDiagramInterpreter, DiagramInterpreter>();
        // services.AddSingleton<IVectorSearchService, VectorSearchService>();
        // services.AddSingleton<ISourceLoader, SourceLoader>();
        // services.AddSingleton<IPromptComposer, PromptComposer>();
        // services.AddSingleton<ITerraformGenerator, TerraformGenerator>();
        // services.AddSingleton<IArtifactStore, ArtifactStore>();
        // services.AddSingleton<IJobRepository, JobRepository>();
        // services.AddSingleton<IJobQueue, JobQueue>();

        // Register workflow steps
        services.AddTransient<IWorkflowStep, ParseDiagramStep>();
        services.AddTransient<IWorkflowStep, RetrieveContextStep>();
        services.AddTransient<IWorkflowStep, GenerateTerraformStep>();
        services.AddTransient<IWorkflowStep, PackageArtifactsStep>();

        // Workflow engine and worker
        services.AddSingleton<WorkflowEngine>();
        services.AddHostedService<Worker>();

        // logging, health etc. are registered by default
    });

await builder.RunConsoleAsync();