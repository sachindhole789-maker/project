using App.Domain.Constants;
using App.Domain.DTO.Common;
using App.Domain.IInterfaces;
using App.Domain.Manager;
namespace App.Infrastructure.Repositories.AuditRepository;
public sealed class AuditRepository(IDbManager dbManager) : IAuditStore
{
    public Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken) => dbManager.ExecuteAsync(StoredProcedureNames.InsertAuditLog, new { entry.RequestId, entry.UserId, entry.Method, entry.Path, entry.StatusCode, entry.DurationMs, entry.Timestamp, entry.RequestBody, entry.ResponseBody }, cancellationToken);
}
