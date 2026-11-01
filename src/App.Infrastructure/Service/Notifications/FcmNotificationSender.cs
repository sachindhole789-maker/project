using App.Domain.IInterfaces;
using FirebaseAdmin.Messaging;
namespace App.Infrastructure.Service.Notifications;
/// <summary>Send a push to a registered FCM device identifier. Keep secrets in workload identity/secret storage and notification data non-sensitive.</summary>
public sealed class FcmNotificationSender(FirebaseMessaging messaging) : INotificationSender
{
    public Task SendAsync(string recipient, string title, string message, CancellationToken cancellationToken = default) => messaging.SendAsync(new Message { Fid = recipient, Notification = new Notification { Title = title, Body = message } }, cancellationToken);
}
