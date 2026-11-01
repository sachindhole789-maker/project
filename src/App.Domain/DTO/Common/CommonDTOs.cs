namespace App.Domain.DTO.Common;
public sealed record PageRequest(int PageNumber = 1, int PageSize = App.Domain.Constants.AppConstants.DefaultPageSize)
{
    public int Offset => Math.Max(0, PageNumber - 1) * Math.Clamp(PageSize, 1, App.Domain.Constants.AppConstants.MaximumPageSize);
    public int Limit => Math.Clamp(PageSize, 1, App.Domain.Constants.AppConstants.MaximumPageSize);
}
public sealed record RepositoryPage<T>(IReadOnlyList<T> Items, long TotalRecords, int PageNumber, int PageSize);
public sealed record AuditEntry(string RequestId, string? UserId, string Method, string Path, int StatusCode, long DurationMs, DateTimeOffset Timestamp, string? RequestBody, string? ResponseBody);
public sealed record UploadReceipt(string Key, string ContentType, long Length, DateTimeOffset UploadedAt);
