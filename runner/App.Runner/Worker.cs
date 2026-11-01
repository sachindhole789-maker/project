using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace App.Runner;
// Replace the periodic sample with an idempotent outbox/cleanup job; use a distributed lock when running multiple replicas.
public sealed class Worker(ILogger<Worker> logger):BackgroundService
{
 protected override async Task ExecuteAsync(CancellationToken stoppingToken){while(!stoppingToken.IsCancellationRequested){logger.LogInformation("Background worker heartbeat at {Time}",DateTimeOffset.UtcNow);await Task.Delay(TimeSpan.FromMinutes(5),stoppingToken);}}
}
