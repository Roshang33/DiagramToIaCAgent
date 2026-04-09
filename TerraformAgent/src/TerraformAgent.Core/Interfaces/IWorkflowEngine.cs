using TerraformAgent.Core.Models;

namespace TerraformAgent.Core.Interfaces;

public interface IWorkflowEngine
{
    Task ExecuteAsync(AgentJob job);
}