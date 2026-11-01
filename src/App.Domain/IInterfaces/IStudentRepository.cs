using App.Domain.DTO.Common;
using App.Domain.DTO.User;
namespace App.Domain.IInterfaces;
public interface IStudentRepository
{
    Task<GetStudentResultDTO?> GetByCustomerIdAsync(GetStudentByCustomerIdRequestDTO request, CancellationToken cancellationToken = default);
    Task<RepositoryPage<GetStudentResultDTO>> GetByFilterAsync(GetStudentByFilterRequestDTO request, CancellationToken cancellationToken = default);
}
