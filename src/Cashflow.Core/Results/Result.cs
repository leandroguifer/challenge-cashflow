namespace Cashflow.Core.Results;

public class Result
{
    protected Result(ResultError? error) => Error = error;

    public bool IsSuccess => Error is null;

    public bool IsFailure => Error is not null;

    public ResultError? Error { get; }

    public static Result Success() => new(null);

    public static Result Failure(ResultError error) => new(error);

    public static Result<T> Success<T>(T value) => new(value, null);

    public static Result<T> Failure<T>(ResultError error) => new(default, error);
}

public sealed class Result<T> : Result
{
    internal Result(T? value, ResultError? error)
        : base(error) => Value = value;

    public T? Value { get; }
}