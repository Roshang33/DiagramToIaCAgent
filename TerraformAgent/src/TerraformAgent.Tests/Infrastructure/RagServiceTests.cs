using TerraformAgent.Infrastructure.RAG;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace TerraformAgent.Tests.Infrastructure;

public class RagServiceTests
{
    [Fact]
    public async Task SearchAsync_ShouldReturnResults()
    {
        var vectorSearch = new VectorSearchService();

        var ragService = new RagService(vectorSearch);

        var results = await ragService.SearchAsync("aks");

        results.Should().NotBeEmpty();
        results.First().Repository.Should().Be("infra-modules");
    }
}