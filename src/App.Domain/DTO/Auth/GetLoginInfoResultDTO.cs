namespace App.Domain.DTO.Auth;
public sealed record GetLoginInfoResultDTO(long UserId, string UserName, string Email, string PasswordHash, bool IsActive);
