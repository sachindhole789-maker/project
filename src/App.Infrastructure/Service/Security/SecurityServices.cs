using App.Domain.Service;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
namespace App.Infrastructure.Service.Security;
public sealed class DataProtectionSecretProtector(IDataProtectionProvider provider) : ISecretProtector
{
    private readonly IDataProtector _protector = provider.CreateProtector("App.Secret.v1");
    public string Protect(string plaintext) => _protector.Protect(plaintext);
    public string Unprotect(string ciphertext) => _protector.Unprotect(ciphertext);
}
public sealed class AspNetPasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> _hasher = new();
    private readonly object _user = new();
    public string Hash(string password) => _hasher.HashPassword(_user, password);
    public bool Verify(string password, string encodedHash) => _hasher.VerifyHashedPassword(_user, encodedHash, password) != PasswordVerificationResult.Failed;
}
