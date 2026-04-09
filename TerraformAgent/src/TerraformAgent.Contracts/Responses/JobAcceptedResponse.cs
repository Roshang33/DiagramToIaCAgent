namespace TerraformAgent.Contracts.Responses;

public class JobAcceptedResponse
{
    public string JobId { get; set; } = string.Empty;

    public string Status { get; set; } = "Accepted";

    public string StatusUrl { get; set; } = string.Empty;
}