namespace App.Domain.Service;
public interface IOtpManager
{
    Task<string> GenerateAsync(string subject, TimeSpan lifetime, CancellationToken cancellationToken = default);
    Task<bool> VerifyAsync(string subject, string code, CancellationToken cancellationToken = default);
}
