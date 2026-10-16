namespace ClinicManagement.Domain.Common;

/// <summary>
/// Non-generic result for commands that don't return data (e.g. Delete, Cancel).
/// MediatR handlers return Result/Result&lt;T&gt; instead of throwing for expected,
/// business-rule failures (validation, not-found, limit-exceeded). Exceptions are
/// reserved for truly unexpected failures and are caught by the global exception
/// middleware in the API layer.
/// </summary>
public class Result
{
    public bool Succeeded { get; }
    public string[] Errors { get; }

    protected Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public static Result Success() => new(true, Array.Empty<string>());

    public static Result Failure(params string[] errors) => new(false, errors);

    public static Result Failure(IEnumerable<string> errors) => new(false, errors);
}

/// <summary>Result that carries a payload on success.</summary>
public class Result<T> : Result
{
    public T? Data { get; }

    protected Result(bool succeeded, T? data, IEnumerable<string> errors)
        : base(succeeded, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, data, Array.Empty<string>());

    public new static Result<T> Failure(params string[] errors) => new(false, default, errors);

    public new static Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);

    public static implicit operator Result<T>(T data) => Success(data);
}
