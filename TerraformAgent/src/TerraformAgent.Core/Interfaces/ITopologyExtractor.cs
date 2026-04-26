namespace TerraformAgent.Core.Interfaces;

public sealed class DetectedEdge
{
    public int FromIconIndex { get; init; }    // index into the list returned by IIconDetector
    public int ToIconIndex { get; init; }
    public string Type { get; init; } = "connection";
    public double Confidence { get; init; }
}

public interface ITopologyExtractor
{
    // Given an image and detected icons, return edges (connector detection).
    Task<List<DetectedEdge>> ExtractEdgesAsync(byte[] imageBytes, List<DetectedIcon> icons, CancellationToken ct);
}