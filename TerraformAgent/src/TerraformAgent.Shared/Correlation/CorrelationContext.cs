namespace TerraformAgent.Shared.Correlation;

public class CorrelationContext
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    public static string? Current
    {
        get => _correlationId.Value;
        set => _correlationId.Value = value;
    }
}