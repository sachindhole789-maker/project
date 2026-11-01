using Amazon.S3;
using Amazon.S3.Model;
using App.Domain.IInterfaces;
using Microsoft.Extensions.Options;
namespace App.Infrastructure.Service.Storage;
public sealed class S3Options
{
    public string BucketName { get; set; } = "";
    public long MaxBytes { get; set; } = 10_000_000;
    public string[] AllowedContentTypes { get; set; } = ["application/pdf", "image/png", "image/jpeg", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"];
}
public sealed class S3ObjectStorage(IAmazonS3 s3, IOptions<S3Options> options, IFileScanner scanner) : IObjectStorage
{
    public async Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken ct)
    {
        var o = options.Value;
        if (string.IsNullOrWhiteSpace(o.BucketName)) throw new InvalidOperationException("S3 bucket is not configured.");
        if (!content.CanSeek) throw new InvalidDataException("A seekable, size-limited upload stream is required.");
        if (!o.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase)) throw new InvalidDataException("Unsupported document type.");
        if (content.Length <= 0 || content.Length > o.MaxBytes) throw new InvalidDataException("File is empty or exceeds the configured size limit.");
        if (!await HasExpectedSignature(content, contentType, ct)) throw new InvalidDataException("File signature does not match its declared document type.");
        content.Position = 0;
        if (!await scanner.IsCleanAsync(content, ct)) throw new InvalidDataException("File did not pass malware scanning.");
        content.Position = 0;
        await s3.PutObjectAsync(new PutObjectRequest { BucketName = o.BucketName, Key = key, InputStream = content, ContentType = contentType, ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256 }, ct);
        return key;
    }
    private static async Task<bool> HasExpectedSignature(Stream stream, string type, CancellationToken ct)
    {
        stream.Position = 0; var b = new byte[8]; var n = await stream.ReadAsync(b.AsMemory(), ct);
        return type switch
        {
            "application/pdf" => n >= 5 && b.AsSpan(0, 5).SequenceEqual("%PDF-"u8),
            "image/png" => n >= 8 && b.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            "image/jpeg" => n >= 3 && b[0] == 255 && b[1] == 216 && b[2] == 255,
            _ => n >= 4 && b[0] == 0x50 && b[1] == 0x4b && b[2] == 0x03 && b[3] == 0x04
        };
    }
}
// Fails closed until a real malware scanner (for example ClamAV or a managed scanning service) is configured.
public sealed class FailClosedFileScanner : IFileScanner { public Task<bool> IsCleanAsync(Stream content, CancellationToken ct) => Task.FromResult(false); }
public sealed class DocumentUploadService(IObjectStorage storage, App.Domain.IInterfaces.IUploadRepository repository) : IDocumentUploadService
{
    public async Task<App.Domain.DTO.Common.UploadReceipt> UploadAsync(string originalFileName, string contentType, Stream content, CancellationToken ct = default)
    {
        if (!content.CanSeek || content.Length <= 0) throw new InvalidDataException("A non-empty, seekable upload stream is required.");
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        var key = $"documents/{DateTimeOffset.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{ext}";
        await storage.UploadAsync(key, content, contentType, ct);
        var receipt = new App.Domain.DTO.Common.UploadReceipt(key, contentType, content.Length, DateTimeOffset.UtcNow);
        await repository.SaveAsync(receipt, ct);
        return receipt;
    }
}
