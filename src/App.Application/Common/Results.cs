using App.Domain.Enums;
namespace App.Application.Common;
public sealed record Result<T>(T? Data, string Message, ResultStatus Status, string StatusCode)
{
    public static Result<T> Success(T data, string message = "Success") => new(data, message, ResultStatus.Success, "200");
    public static Result<T> Failure(string message, string statusCode = "400", ResultStatus status = ResultStatus.Failure) => new(default, message, status, statusCode);
}
public sealed record PageMetadata(long TotalRecords, int PageNumber, int PageSize)
{
    public long TotalPages => PageSize <= 0 ? 0 : (long)Math.Ceiling(TotalRecords / (double)PageSize);
}
public sealed record PaginatedResult<T>(IReadOnlyList<T> Items, PageMetadata Pagination, string Message, ResultStatus Status, string StatusCode);
