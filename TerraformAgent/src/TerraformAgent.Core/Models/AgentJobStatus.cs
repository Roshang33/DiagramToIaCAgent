using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraformAgent.Core.Models
{
    public enum AgentJobstatus
    {
        Pending,
        RetrievingContext,
        GeneratingTerraform,
        Validating
    }
}
