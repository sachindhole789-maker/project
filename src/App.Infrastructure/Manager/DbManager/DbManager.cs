using System.Data;
using System.Data.Common;
using App.Domain.DTO.Common;
using App.Domain.Manager;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
namespace App.Infrastructure.Manager.DbManager;
public sealed class DatabaseOptions { public string Provider { get; set; } = "SqlServer"; public string ConnectionString { get; set; } = ""; }
public sealed class DbConnectionFactory(IOptions<DatabaseOptions> options)
{
    public IDbConnection Create() => options.Value.Provider.ToLowerInvariant() switch
    {
        "sqlserver" => new SqlConnection(options.Value.ConnectionString),
        "postgres" or "postgresql" or "awsrds" => new NpgsqlConnection(options.Value.ConnectionString),
        "oracle" => new OracleConnection(options.Value.ConnectionString),
        _ => throw new InvalidOperationException($"Unsupported database provider '{options.Value.Provider}'.")
    };
}
public sealed class DapperDbManager(DbConnectionFactory factory) : IDbManager
{
    public async Task<IDbConnection> OpenConnectionAsync(CancellationToken ct = default) { var c = factory.Create(); if (c is DbConnection dc) await dc.OpenAsync(ct); else c.Open(); return c; }
    public async Task<IReadOnlyList<TEntity>> QueryAsync<TEntity>(string procedure, object? parameters, CancellationToken ct = default) { using var c = await OpenConnectionAsync(ct); var cmd = new CommandDefinition(procedure, parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct); return (await c.QueryAsync<TEntity>(cmd)).AsList(); }
    public async Task<TEntity?> QuerySingleOrDefaultAsync<TEntity>(string procedure, object? parameters, CancellationToken ct = default) { using var c = await OpenConnectionAsync(ct); return await c.QuerySingleOrDefaultAsync<TEntity>(new CommandDefinition(procedure, parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct)); }
    public async Task<int> ExecuteAsync(string procedure, object? parameters, CancellationToken ct = default) { using var c = await OpenConnectionAsync(ct); return await c.ExecuteAsync(new CommandDefinition(procedure, parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct)); }
    public async Task<RepositoryPage<TEntity>> QueryPageAsync<TEntity>(string procedure, object? parameters, PageRequest page, CancellationToken ct = default) { var p = new DynamicParameters(parameters); p.Add("Offset", page.Offset); p.Add("PageSize", page.Limit); using var c = await OpenConnectionAsync(ct); using var grid = await c.QueryMultipleAsync(new CommandDefinition(procedure, p, commandType: CommandType.StoredProcedure, cancellationToken: ct)); var rows = (await grid.ReadAsync<TEntity>()).AsList(); var total = await grid.ReadSingleAsync<long>(); return new(rows, total, page.PageNumber, page.Limit); }
}
