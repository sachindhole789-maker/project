using App.Domain.Constants;
using App.Domain.DTO.Auth;
using App.Domain.Entities.Auth;
using App.Domain.IInterfaces;
using App.Domain.Manager;
namespace App.Infrastructure.Repositories.AuthRepository;
public sealed class AuthRepository(IDbManager dbManager) : IAuthRepository
{
    public async Task<GetLoginInfoResultDTO?> GetLoginAsync(GetLoginInfoRequestDTO request, CancellationToken cancellationToken = default)
    {
        var entity = await dbManager.QuerySingleOrDefaultAsync<UserLoginEntity>(StoredProcedureNames.GetLoginInfo, request, cancellationToken);
        return entity is null ? null : new GetLoginInfoResultDTO(entity.UserId, entity.UserName, entity.Email, entity.PasswordHash, entity.IsActive);
    }
}
