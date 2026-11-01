namespace App.Domain.IInterfaces;
public interface INotificationSender { Task SendAsync(string recipient, string title, string message, CancellationToken cancellationToken = default); }
