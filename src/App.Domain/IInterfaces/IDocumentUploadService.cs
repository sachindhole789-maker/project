using App.Domain.DTO.Common;
namespace App.Domain.IInterfaces;
public interface IDocumentUploadService { Task<UploadReceipt> UploadAsync(string originalFileName, string contentType, Stream content, CancellationToken cancellationToken = default); }
