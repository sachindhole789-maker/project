using App.Domain.DTO.Auth;
namespace App.Domain.IInterfaces;
public interface IAuthRepository
{
    Task<GetLoginInfoResultDTO?> GetLoginAsync(GetLoginInfoRequestDTO request, CancellationToken cancellationToken = default);
}
