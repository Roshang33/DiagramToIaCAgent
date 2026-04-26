namespace TerraformAgent.Core.Interfaces;

public interface IOcrService
{
    // Returns tuples of extracted text and bounding box [x,y,width,height]
    Task<List<(string Text, int[] Bbox, double Confidence)>> ExtractTextAsync(byte[] imageBytes, CancellationToken ct);
}