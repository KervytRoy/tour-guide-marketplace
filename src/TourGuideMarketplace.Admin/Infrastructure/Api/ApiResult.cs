namespace TourGuideMarketplace.Admin.Infrastructure.Api;

public sealed record ApiResult(bool Succeeded, IReadOnlyCollection<string> Errors)
{
    public static ApiResult Success() => new(true, []);
    public static ApiResult Failure(params string[] errors) => new(false, errors);
    public static ApiResult Failure(IReadOnlyCollection<string> errors) => new(false, errors);
}

public sealed record ApiResult<T>(bool Succeeded, T? Value, IReadOnlyCollection<string> Errors)
{
    public static ApiResult<T> Success(T value) => new(true, value, []);
    public static ApiResult<T> Failure(params string[] errors) => new(false, default, errors);
    public static ApiResult<T> Failure(IReadOnlyCollection<string> errors) => new(false, default, errors);
}

