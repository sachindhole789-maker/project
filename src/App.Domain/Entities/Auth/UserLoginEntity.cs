namespace App.Domain.Entities.Auth;
// Properties map to stored procedure column names. Alias SQL columns to these names where necessary.
public sealed class UserLoginEntity
{
    public long UserId { get; init; }
    public string UserName { get; init; } = "";
    public string Email { get; init; } = "";
    public string PasswordHash { get; init; } = "";
    public bool IsActive { get; init; }
}
