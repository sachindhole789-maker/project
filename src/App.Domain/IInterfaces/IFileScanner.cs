namespace App.Domain.IInterfaces;
public interface IFileScanner { Task<bool> IsCleanAsync(Stream content, CancellationToken cancellationToken); }
