using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TerraformAgent.Persistence.Repositories;
using TerraformAgent.Persistence.Storage;

namespace TerraformAgent.Persistence.DependencyInjection;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["Storage:ConnectionString"];

        var table = TableStorageFactory.Create(connectionString, "TerraformJobs");

        services.AddSingleton<TableClient>(table);

        services.AddScoped<IJobRepository, JobRepository>();

        return services;
    }
}