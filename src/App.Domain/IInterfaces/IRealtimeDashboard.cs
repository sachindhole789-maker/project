namespace App.Domain.IInterfaces;
public interface IRealtimeDashboard { Task PublishAsync<T>(string topic, T data, CancellationToken cancellationToken = default); }
