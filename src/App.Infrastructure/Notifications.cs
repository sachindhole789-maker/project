using App.Domain.Data;
using FirebaseAdmin.Messaging;
namespace App.Infrastructure.Notifications;
/// <summary>Fires one Firebase Cloud Messaging push to a registered device token. Keep credentials in workload identity or secret storage.</summary>
public sealed class FcmNotificationSender(FirebaseMessaging messaging):INotificationSender
{
 public Task SendAsync(string recipient,string title,string message,CancellationToken cancellationToken=default)=>messaging.SendAsync(new Message{Fid=recipient,Notification=new Notification{Title=title,Body=message}},cancellationToken);
}

