namespace Munaqasat.Web.Services.Common;

public readonly record struct OperationResult
{
    private OperationResult(bool succeeded, string? error)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public bool Succeeded { get; }
    public string? Error { get; }

    public static OperationResult Success() => new(true, null);

    public static OperationResult Failure(string error) => new(false, error);
}

public readonly record struct OperationResult<T>
{
    private OperationResult(bool succeeded, T? value, string? error)
    {
        Succeeded = succeeded;
        Value = value;
        Error = error;
    }

    public bool Succeeded { get; }
    public T? Value { get; }
    public string? Error { get; }

    public static OperationResult<T> Success(T value) => new(true, value, null);

    public static OperationResult<T> Failure(string error) => new(false, default, error);
}
