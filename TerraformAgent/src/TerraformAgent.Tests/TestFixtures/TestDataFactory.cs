using System.Collections.Generic;
using System;
using TerraformAgent.Core.Models;

namespace TerraformAgent.Tests.TestFixtures;

public static class TestDataFactory
{
    public static AgentJob CreateJob()
    {
        return new AgentJob
        {
            JobId = Guid.NewGuid().ToString(),
            Prompt = "Create an AKS cluster with VNet",
            DiagramUrl = "https://test/diagram.png",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static List<RagResult> CreateRagResults()
    {
        return new List<RagResult>
        {
            new RagResult
            {
                Repository = "infra-modules",
                FilePath = "aks/cluster.tf",
                SourceUrl = "https://github.com/repo/aks.tf",
                Snippet = "resource \"azurerm_kubernetes_cluster\" {}",
                Score = 0.91
            }
        };
    }
}