namespace Cashflow.Core.Results;

public sealed class Result<T> : Result
{
    internal Result(T? value, ResultError? error)
        : base(error) => Value = value;

    public T? Value { get; }
}
