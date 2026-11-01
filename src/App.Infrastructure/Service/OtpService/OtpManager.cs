using System.Security.Cryptography;
using System.Text;
using App.Domain.Service;
using Microsoft.Extensions.Caching.Memory;
namespace App.Infrastructure.Service.OtpService;
/// <summary>Single-process OTP sample. Use a distributed cache/shared store and per-subject request throttling when running multiple API replicas.</summary>
public sealed class OtpManager(IMemoryCache cache, ISecretProtector protector) : IOtpManager
{
    private sealed record OtpState(string ProtectedCode, DateTimeOffset ExpiresAt, int FailedAttempts);
    public Task<string> GenerateAsync(string subject, TimeSpan lifetime, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Subject is required.", nameof(subject));
        if (lifetime <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(lifetime));
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var expiresAt = DateTimeOffset.UtcNow.Add(lifetime);
        cache.Set(CacheKey(subject), new OtpState(protector.Protect(code), expiresAt, 0), new MemoryCacheEntryOptions { AbsoluteExpiration = expiresAt });
        return Task.FromResult(code);
    }
    public Task<bool> VerifyAsync(string subject, string code, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = CacheKey(subject);
        if (string.IsNullOrWhiteSpace(code) || !cache.TryGetValue<OtpState>(key, out var state) || state is null || state.ExpiresAt <= DateTimeOffset.UtcNow) return Task.FromResult(false);
        var expected = Encoding.UTF8.GetBytes(protector.Unprotect(state.ProtectedCode));
        var actual = Encoding.UTF8.GetBytes(code);
        var valid = expected.Length == actual.Length && CryptographicOperations.FixedTimeEquals(expected, actual);
        if (valid) cache.Remove(key);
        else if (state.FailedAttempts + 1 >= 5) cache.Remove(key);
        else cache.Set(key, state with { FailedAttempts = state.FailedAttempts + 1 }, new MemoryCacheEntryOptions { AbsoluteExpiration = state.ExpiresAt });
        return Task.FromResult(valid);
    }
    private static string CacheKey(string subject) => $"otp:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(subject.Trim().ToLowerInvariant())))}";
}
