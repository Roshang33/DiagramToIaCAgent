using Microsoft.AspNetCore.Mvc;
using Moq;
using TerraformAgent.Api.Controllers;
using TerraformAgent.Persistence.Repositories;
using FluentAssertions;
using System.Threading.Tasks;
using System;
using Xunit;

namespace TerraformAgent.Tests.Api;

public class TerraformJobsControllerTests
{
    [Fact]
    public async Task GetStatus_ShouldReturnNotFound_WhenJobMissing()
    {
        var repo = new Mock<IJobRepository>();

        repo.Setup(x => x.GetJobAsync(It.IsAny<string>()))
            .ReturnsAsync((Core.Models.AgentJob?)null);

        var controller = new TerraformJobsController(
            repo.Object,
            null!,
            null!,
            null!);

        var result = await controller.GetStatus(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }
}