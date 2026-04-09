namespace TerraformAgent.Shared.Results;

public class ResultError
{
    public string Code { get; set; } = "";

    public string Message { get; set; } = "";

    public static ResultError NotFound(string message)
        => new() { Code = "NOT_FOUND", Message = message };

    public static ResultError Validation(string message)
        => new() { Code = "VALIDATION_ERROR", Message = message };

    public static ResultError Internal(string message)
        => new() { Code = "INTERNAL_ERROR", Message = message };
}