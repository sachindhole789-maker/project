using App.Domain.DTO.Common;
namespace App.Domain.IInterfaces;
public interface IUploadRepository { Task SaveAsync(UploadReceipt receipt, CancellationToken cancellationToken); }
