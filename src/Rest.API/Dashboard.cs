using Microsoft.AspNetCore.SignalR;
using App.Domain.IInterfaces;
namespace Rest.API.Hubs;
public sealed class DashboardHub : Hub { }
public sealed class SignalRDashboard(IHubContext<DashboardHub> hub) : IRealtimeDashboard
{
    public Task PublishAsync<T>(string topic, T data, CancellationToken ct = default) => hub.Clients.All.SendAsync(topic, data, ct);
}
