namespace App.Domain.Service;
public interface ISecretProtector { string Protect(string plaintext); string Unprotect(string ciphertext); }
