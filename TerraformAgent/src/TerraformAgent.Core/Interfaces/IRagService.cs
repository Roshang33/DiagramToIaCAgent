using TerraformAgent.Core.Models;

namespace TerraformAgent.Core.Interfaces;

public interface IRagService
{
    Task<List<RagResult>> SearchAsync(string query);
}