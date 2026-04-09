namespace TerraformAgent.Core.Constants;

public static class JobStatus
{
    public const string Pending = "Pending";
    public const string RetrievingContext = "RetrievingContext";
    public const string GeneratingTerraform = "GeneratingTerraform";
    public const string Validating = "Validating";
    public const string Completed = "Completed";
    public const string Failed = "Failed";

    
}