using System.Data;
using App.Domain.Results;

namespace App.Domain.Data;

public sealed record PageRequest(int PageNumber = 1, int PageSize = 20)
{
    public int Offset => Math.Max(0, PageNumber - 1) * Math.Clamp(PageSize, 1, 200);
    public int Limit => Math.Clamp(PageSize, 1, 200);
}
public sealed record RepositoryPage<T>(IReadOnlyList<T> Items, long TotalRecords, int PageNumber, int PageSize);

/// <summary>Provider-neutral stored procedure contract. Configure one provider per deployment environment.</summary>
public interface IDbManager
{
    Task<IReadOnlyList<T>> QueryAsync<T>(string procedure, object? parameters, CancellationToken cancellationToken = default);
    Task<T?> QuerySingleOrDefaultAsync<T>(string procedure, object? parameters, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(string procedure, object? parameters, CancellationToken cancellationToken = default);
    Task<RepositoryPage<T>> QueryPageAsync<T>(string procedure, object? parameters, PageRequest page, CancellationToken cancellationToken = default);
    Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}

public interface IUserInfoProvider { string? UserId { get; } }
public interface ISecretProtector { string Protect(string plaintext); string Unprotect(string ciphertext); }
public interface IPasswordHasher { string Hash(string password); bool Verify(string password, string encodedHash); }
public interface IAuditStore { Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken); }
public sealed record AuditEntry(string RequestId, string? UserId, string Method, string Path, int StatusCode, long DurationMs, DateTimeOffset Timestamp, string? RequestBody, string? ResponseBody);

public interface IFileScanner { Task<bool> IsCleanAsync(Stream content, CancellationToken cancellationToken); }
public interface IObjectStorage { Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken); }
public sealed record UploadReceipt(string Key, string ContentType, long Length, DateTimeOffset UploadedAt);
public interface IUploadRepository { Task SaveAsync(UploadReceipt receipt, CancellationToken cancellationToken); }
public interface IDocumentUploadService { Task<UploadReceipt> UploadAsync(string originalFileName, string contentType, Stream content, CancellationToken cancellationToken = default); }
public interface IRealtimeDashboard { Task PublishAsync<T>(string topic, T data, CancellationToken cancellationToken = default); }
public interface INotificationSender { Task SendAsync(string recipient, string title, string message, CancellationToken cancellationToken = default); }


