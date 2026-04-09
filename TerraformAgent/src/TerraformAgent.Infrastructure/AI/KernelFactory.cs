using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace TerraformAgent.Infrastructure.AI;

public static class KernelFactory
{
    public static Kernel CreateKernel(IConfiguration configuration)
    {
        var builder = Kernel.CreateBuilder();

        var endpoint = configuration["OpenAI:Endpoint"];
        var apiKey = configuration["OpenAI:ApiKey"];
        var model = configuration["OpenAI:Model"];

        builder.AddOpenAIChatCompletion(model, apiKey);

        return builder.Build();
    }
}