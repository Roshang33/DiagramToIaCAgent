using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using TerraformAgent.Core.Interfaces;
using TerraformAgent.Infrastructure.AI;
using TerraformAgent.Infrastructure.RAG;
using TerraformAgent.Infrastructure.SourceControl;
using TerraformAgent.Infrastructure.Vision;

namespace TerraformAgent.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<Kernel>(sp =>
            KernelFactory.CreateKernel(configuration));

        services.AddHttpClient();

        services.AddSingleton<VectorSearchService>();

        services.AddScoped<IRagService, RagService>();

        services.AddScoped<ISourceLoader, SourceLoader>();

        services.AddScoped<IDiagramAnalyzer, DiagramAnalyzer>();

        services.AddScoped<ITerraformGenerator, TerraformGenerator>();

        return services;
    }
}