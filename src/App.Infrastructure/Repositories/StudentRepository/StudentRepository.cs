using App.Domain.Constants;
using App.Domain.DTO.Common;
using App.Domain.DTO.User;
using App.Domain.Entities.User;
using App.Domain.IInterfaces;
using App.Domain.Manager;
namespace App.Infrastructure.Repositories.StudentRepository;
public sealed class StudentRepository(IDbManager dbManager) : IStudentRepository
{
    public async Task<GetStudentResultDTO?> GetByCustomerIdAsync(GetStudentByCustomerIdRequestDTO request, CancellationToken cancellationToken = default)
    {
        var entity = await dbManager.QuerySingleOrDefaultAsync<StudentEntity>(StoredProcedureNames.GetStudentByCustomerId, request, cancellationToken);
        return entity is null ? null : Map(entity);
    }
    public async Task<RepositoryPage<GetStudentResultDTO>> GetByFilterAsync(GetStudentByFilterRequestDTO request, CancellationToken cancellationToken = default)
    {
        var page = await dbManager.QueryPageAsync<StudentEntity>(StoredProcedureNames.GetStudentsByFilter, request, new PageRequest(request.PageNumber, request.PageSize), cancellationToken);
        return new RepositoryPage<GetStudentResultDTO>(page.Items.Select(Map).ToArray(), page.TotalRecords, page.PageNumber, page.PageSize);
    }
    private static GetStudentResultDTO Map(StudentEntity entity) => new(entity.CustomerId, entity.Name, entity.Email, entity.Mobile, entity.City);
}
