namespace TerraformAgent.Shared.Utilities;

public static class TokenEstimator
{
    public static int Estimate(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        return text.Length / 4;
    }

    public static int Estimate(IEnumerable<string> texts)
    {
        return texts.Sum(Estimate);
    }
}