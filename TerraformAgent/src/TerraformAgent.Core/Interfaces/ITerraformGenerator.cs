using TerraformAgent.Core.Models;

namespace TerraformAgent.Core.Interfaces;

public interface ITerraformGenerator
{
    Task<TerraformResult> GenerateAsync(string intent, List<RagResult> v, CancellationToken ct);
  
}