namespace TerraformAgent.Core.Interfaces;

public sealed class DetectedIcon
{
    public string Type { get; init; } = string.Empty;    // e.g. "vnet_icon", "sql_icon"
    public int[] Bbox { get; init; } = new int[4];       // [x,y,w,h]
    public double Confidence { get; init; }
}

public interface IIconDetector
{
    // Returns detected icons (type + bbox + confidence)
    Task<List<DetectedIcon>> DetectIconsAsync(byte[] imageBytes, CancellationToken ct);
}