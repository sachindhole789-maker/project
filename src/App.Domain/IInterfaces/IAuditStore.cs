using App.Domain.DTO.Common;
namespace App.Domain.IInterfaces;
public interface IAuditStore { Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken); }
