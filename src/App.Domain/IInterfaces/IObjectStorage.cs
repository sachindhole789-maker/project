namespace App.Domain.IInterfaces;
public interface IObjectStorage { Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken); }
