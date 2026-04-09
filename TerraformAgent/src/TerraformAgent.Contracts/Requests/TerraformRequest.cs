namespace TerraformAgent.Contracts.Requests;

public class TerraformRequest
{
    /// <summary>
    /// Optional architecture diagram URL (PNG, JPG, etc.)
    /// </summary>
    public string? DiagramUrl { get; set; }

    /// <summary>
    /// Natural language prompt describing infrastructure
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// Optional repository to use for RAG
    /// </summary>
    public string? Repository { get; set; }

    /// <summary>
    /// Additional search keywords
    /// </summary>
    public List<string>? Keywords { get; set; }
}