using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraformAgent.Core.Models;

public sealed class DiagramEdge
{
    public string FromNodeId { get; init; } = string.Empty;
    public string ToNodeId { get; init; } = string.Empty;
    public string Type { get; init; } = "connection";            // e.g. "arrow" or "line"
    public double Confidence { get; init; }
}
