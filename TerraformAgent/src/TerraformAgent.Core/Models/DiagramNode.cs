
namespace TerraformAgent.Core.Models;

public sealed class DiagramNode
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Type { get; init; } = string.Empty;            // detector type (e.g. "app_service_icon")
    public string Label { get; set; } = string.Empty;            // nearest OCR text or extracted label
    public double Confidence { get; init; }                      // detector confidence
    public int[] Bbox { get; init; } = new int[4];               // [x, y, width, height]
}