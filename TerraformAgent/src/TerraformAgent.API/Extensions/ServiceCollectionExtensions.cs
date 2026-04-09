using TerraformAgent.Api.Services;
using TerraformAgent.Persistence.Repositories;

namespace TerraformAgent.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTerraformAgentServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IJobRepository, JobRepository>();

        services.AddSingleton<JobQueueService>();
        services.AddSingleton<FileStorageService>();

        return services;
    }
}
