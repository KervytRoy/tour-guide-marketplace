namespace TourGuideMarketplace.Application.Common.Models;

public class Result
{
    protected Result(bool succeeded, IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }
    public IReadOnlyCollection<string> Errors { get; }

    public static Result Success() => new(true, []);

    public static Result Failure(params string[] errors) => new(false, errors);
}

public sealed class Result<T> : Result
{
    private Result(bool succeeded, T? value, IReadOnlyCollection<string> errors)
        : base(succeeded, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(true, value, []);

    public static new Result<T> Failure(params string[] errors) => new(false, default, errors);
}
