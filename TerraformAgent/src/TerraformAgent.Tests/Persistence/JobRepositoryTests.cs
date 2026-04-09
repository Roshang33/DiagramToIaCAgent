using Azure.Data.Tables;
using TerraformAgent.Persistence.Repositories;
using TerraformAgent.Persistence.Storage;
using TerraformAgent.Tests.TestFixtures;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace TerraformAgent.Tests.Persistence;

public class JobRepositoryTests
{
    [Fact]
    public async Task CreateJob_ShouldPersistJob()
    {
        var table = TableStorageFactory.Create(
            "UseDevelopmentStorage=true",
            "TestTerraformJobs");

        var repo = new JobRepository(table);

        var job = TestDataFactory.CreateJob();

        await repo.CreateJobAsync(job);

        var stored = await repo.GetJobAsync(job.JobId);

        stored.Should().NotBeNull();
        stored!.Prompt.Should().Be(job.Prompt);
    }
}