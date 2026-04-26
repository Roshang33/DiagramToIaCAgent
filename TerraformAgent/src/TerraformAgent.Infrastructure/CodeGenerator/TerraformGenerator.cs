using Microsoft.SemanticKernel;
using TerraformAgent.Core.Interfaces;
using TerraformAgent.Core.Models;
using TerraformAgent.Infrastructure.AI;
using System.Text;

namespace TerraformAgent.Infrastructure.AI;

public class TerraformGenerator : ITerraformGenerator
{
    private readonly Kernel _kernel;

    public TerraformGenerator(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<TerraformResult> GenerateAsync(
        string userPrompt,
        List<RagResult> ragContext,
        CancellationToken ct)
    {
        var contextBuilder = new StringBuilder();

        foreach (var item in ragContext)
        {
            contextBuilder.AppendLine(item.Snippet);
            contextBuilder.AppendLine("-----");
        }

        var function = _kernel.CreateFunctionFromPrompt(
            PromptTemplates.TerraformGeneration);

        var result = await _kernel.InvokeAsync(
            function,
            new()
            {
                ["input"] = userPrompt,
                ["context"] = contextBuilder.ToString()
            });

        return new TerraformResult
        {
            TerraformCode = result.ToString()
        };
    }
}