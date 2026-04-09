namespace TerraformAgent.Shared.Results;

public class Result<T>
{
    public bool Success { get; private set; }

    public T? Value { get; private set; }

    public ResultError? Error { get; private set; }

    private Result(T value)
    {
        Success = true;
        Value = value;
    }

    private Result(ResultError error)
    {
        Success = false;
        Error = error;
    }

    public static Result<T> Ok(T value)
        => new(value);

    public static Result<T> Fail(ResultError error)
        => new(error);
}