using App.Domain.Constants;
using App.Domain.DTO.Common;
using App.Domain.IInterfaces;
using App.Domain.Manager;
namespace App.Infrastructure.Repositories.UploadRepository;
public sealed class UploadRepository(IDbManager dbManager) : IUploadRepository
{
    public Task SaveAsync(UploadReceipt receipt, CancellationToken cancellationToken) => dbManager.ExecuteAsync(StoredProcedureNames.SaveUploadedFile, new { receipt.Key, receipt.ContentType, receipt.Length, receipt.UploadedAt }, cancellationToken);
}
