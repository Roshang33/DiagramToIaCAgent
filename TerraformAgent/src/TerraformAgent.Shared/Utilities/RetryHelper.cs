namespace TerraformAgent.Shared.Utilities;

public static class RetryHelper
{
    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        int retries = 3,
        int delayMs = 500)
    {
        for (var attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                return await action();
            }
            catch when (attempt < retries)
            {
                await Task.Delay(delayMs * attempt);
            }
        }

        throw new Exception("Retry limit exceeded");
    }
}