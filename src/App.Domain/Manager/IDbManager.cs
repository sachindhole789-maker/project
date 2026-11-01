using System.Data;
using App.Domain.DTO.Common;
namespace App.Domain.Manager;
/// <summary>Provider-neutral Dapper contract. Implementations accept DTO parameters and map stored-procedure columns to entities.</summary>
public interface IDbManager
{
    Task<IReadOnlyList<TEntity>> QueryAsync<TEntity>(string procedure, object? parameters, CancellationToken cancellationToken = default);
    Task<TEntity?> QuerySingleOrDefaultAsync<TEntity>(string procedure, object? parameters, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(string procedure, object? parameters, CancellationToken cancellationToken = default);
    Task<RepositoryPage<TEntity>> QueryPageAsync<TEntity>(string procedure, object? parameters, PageRequest page, CancellationToken cancellationToken = default);
    Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
