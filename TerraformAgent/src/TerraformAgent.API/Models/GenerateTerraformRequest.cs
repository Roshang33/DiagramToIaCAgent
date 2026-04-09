using Microsoft.AspNetCore.Http;

namespace TerraformAgent.Api.Models;

public class GenerateTerraformRequest
{
    public string Prompt { get; set; } = string.Empty;

    public IFormFile? Diagram { get; set; }
}
