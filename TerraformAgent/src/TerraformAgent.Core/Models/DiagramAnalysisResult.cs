using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraformAgent.Core.Models;

public sealed class DiagramAnalysisResult
{
    public List<DiagramNode> Nodes { get; init; } = new();
    public List<DiagramEdge> Edges { get; init; } = new();
}
