using App.Domain.Data;
namespace App.Infrastructure.Auditing;
public sealed class DatabaseAuditStore(IDbManager db):IAuditStore { public Task WriteAsync(AuditEntry e,CancellationToken ct)=>db.ExecuteAsync("InsertAuditLog",new{e.RequestId,e.UserId,e.Method,e.Path,e.StatusCode,e.DurationMs,e.Timestamp,e.RequestBody,e.ResponseBody},ct); }
