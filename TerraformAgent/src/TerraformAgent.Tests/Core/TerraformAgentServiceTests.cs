using Moq;
using TerraformAgent.Core.Interfaces;
using TerraformAgent.Core.Services;
using TerraformAgent.Tests.TestFixtures;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TerraformAgent.Tests.Core;

public class TerraformAgentServiceTests
{
    [Fact]
    public async Task ProcessJobAsync_ShouldGenerateTerraform()
    {
        var diagramAnalyzer = new Mock<IDiagramAnalyzer>();
        var ragService = new Mock<IRagService>();
        var sourceLoader = new Mock<ISourceLoader>();
        var terraformGenerator = new Mock<ITerraformGenerator>();

        diagramAnalyzer
            .Setup(x => x.ExtractInfrastructureKeywordsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<string> { "aks", "vnet" });

        ragService
            .Setup(x => x.SearchAsync(It.IsAny<string>()))
            .ReturnsAsync(TestDataFactory.CreateRagResults());

        sourceLoader
            .Setup(x => x.LoadFileContentAsync(It.IsAny<string>()))
            .ReturnsAsync("resource \"azurerm_kubernetes_cluster\" {}");

        terraformGenerator
            .Setup(x => x.GenerateAsync(It.IsAny<string>(), It.IsAny<List<Core.Models.RagResult>>()))
            .ReturnsAsync(new Core.Models.TerraformResult
            {
                TerraformCode = "resource \"azurerm_kubernetes_cluster\" {}"
            });

        var service = new TerraformAgentService(
            diagramAnalyzer.Object,
            ragService.Object,
            sourceLoader.Object,
            terraformGenerator.Object);

        var job = TestDataFactory.CreateJob();

        var result = await service.ProcessJobAsync(job);

        result.TerraformCode.Should().Contain("azurerm_kubernetes_cluster");
    }
}