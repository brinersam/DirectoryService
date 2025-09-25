using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Framework.DbConnection;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text;

namespace DirectoryService.Infrastructure.Database.Repositories;
public class PositionRepository : IPositionRepository
{
    private readonly ILogger<PositionRepository> _logger;
    private readonly AppDb _db;

    public PositionRepository(
        ILogger<PositionRepository> logger,
        AppDb connection)
    {
        _logger = logger;
        _db = connection;
    }

    public async Task<Result<Position, Error>> GetPositionAsync(
        Guid? id = null,
        string name = "",
        bool active = true,
        CancellationToken ct = default)
    {
        try
        {
            var sql = new StringBuilder($"SELECT * FROM {DbTables.Positions} WHERE 1=1");
            var parameters = new DynamicParameters();

            if (id is null && String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("All arguments are null!");

            #region WHERE query
            if (id.HasValue)
            {
                parameters.Add("Id", id.Value);
                sql.Append(" AND id = @Id");
            }

            if (String.IsNullOrWhiteSpace(name) == false)
            {
                parameters.Add("Name", name);
                sql.Append(" AND name = @Name");
            }

            parameters.Add("IsActive", active);
            sql.Append(" AND is_active = @IsActive");
            #endregion query

            var cmd = new CommandDefinition(sql.ToString(), parameters, transaction: _db.Transaction, cancellationToken: ct);

            var dataModel = await _db.Connection.QuerySingleOrDefaultAsync<Position>(cmd);
            if (dataModel is null)
                return Errors.General.NotFound(typeof(Position));

            var departmentGuidsRes = await GetRelatedDepartmentIds(dataModel.Id, ct);
            if (departmentGuidsRes.IsFailure)
                return departmentGuidsRes.Error;

            var result = new Position
            (
                dataModel.Id,
                dataModel.Name,
                dataModel.Description,
                dataModel.IsActive,
                dataModel.CreatedAtUtc,
                dataModel.UpdatedAtUtc,
                departmentGuidsRes.Value
            );

            return result;
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    public async Task<Result<IEnumerable<Guid>, Error>> GetRelatedDepartmentIds(Guid positionId, CancellationToken ct = default)
    {
        try
        {
            var guidsql = $"SELECT department_id FROM {DbTables.Departments_Positions} WHERE position_id = @Id";
            var cmd = new CommandDefinition(guidsql, new { Id = positionId }, transaction: _db.Transaction, cancellationToken: ct);
            return (await _db.Connection.QueryAsync<Guid>(cmd)).ToList();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    public async Task<UnitResult<Error>> AddPositionAsync(Position position, CancellationToken ct = default)
    {
        try
        {
            var sql = @$"INSERT INTO {DbTables.Positions} 
					(id, name, description, is_active, created_at_utc, updated_at_utc) 
					VALUES 
					(@Id, @Name, @Description, @IsActive, @CreatedAtUtc, @UpdatedAtUtc)";

            var cmd = new CommandDefinition(sql, position, _db.Transaction, cancellationToken: ct);

            using var transaction = _db.OpenTransactionIfNotOngoing().Value;

            var rowsaffected = await _db.Connection.ExecuteAsync(cmd);
            if (rowsaffected <= 0)
                return Errors.Database.DBRowsAffectedError<Position>(rowsaffected, 1);

            var synced = await SyncPositionWithDepartments(position, ct);
            if (synced.IsFailure)
                return synced.Error;

            transaction.Commit();

            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    public async Task<UnitResult<Error>> UpdatePositionAsync(Position position, CancellationToken ct = default)
    {
        try
        {
            var sql = @$"UPDATE {DbTables.Positions} SET
					name = @Name,
					description = @Description,
					is_active = @IsActive,
					updated_at_utc = @UpdatedAtUtc
					WHERE id = @Id";

            using var transaction = _db.OpenTransactionIfNotOngoing().Value;

            var cmd = new CommandDefinition(sql, position, _db.Transaction, cancellationToken: ct);

            var rowsaffected = await _db.Connection.ExecuteAsync(cmd);
            if (rowsaffected <= 0)
                return Errors.Database.DBRowsAffectedError<Position>(rowsaffected, 1);

            var synced = await SyncPositionWithDepartments(position, ct);
            if (synced.IsFailure)
                return synced.Error;

            transaction.Commit();

            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    public async Task<UnitResult<Error>> SyncPositionWithDepartments(Position position, CancellationToken ct = default)
    {
        try
        {
            var dbDepartmentIdsRes = await GetRelatedDepartmentIds(position.Id, ct);
            if (dbDepartmentIdsRes.IsFailure)
                return dbDepartmentIdsRes.Error;

            var departmentsRelationsToDelete = dbDepartmentIdsRes.Value.Where(x => position.DepartmentIds.Contains(x) is false).ToArray();
            var departmentsRelationsToAdd = position.DepartmentIds.Where(x => dbDepartmentIdsRes.Value.Contains(x) is false).ToArray();

            if (!departmentsRelationsToAdd.Any() && !departmentsRelationsToDelete.Any())
                return Result.Success<Error>();

            var parameters = new DynamicParameters();
            parameters.Add("PositionId", position.Id);

            var sql = new StringBuilder();

            var sqlCmds = new List<string>();
            if (departmentsRelationsToDelete.Any())
                sqlCmds.Add(CreateDeleteSql(departmentsRelationsToDelete, parameters));

            if (departmentsRelationsToAdd.Any())
                sqlCmds.Add(CreateInsertSql(departmentsRelationsToAdd, parameters));

            sql.Append(String.Join(';', sqlCmds));
            sql.Append(";");

            var cmd = new CommandDefinition(sql.ToString(), parameters, transaction: _db.Transaction, cancellationToken: ct);

            var rowsAffected = await _db.Connection.ExecuteAsync(cmd);
            var expectedAffected = departmentsRelationsToDelete.Length + departmentsRelationsToAdd.Length;
            if (rowsAffected != expectedAffected)
                return Errors.Database.DBRowsAffectedError<Position>(rowsAffected, expectedAffected);

            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    private static string CreateInsertSql(Guid[] departmentsRelationsToAdd, DynamicParameters parameters)
    {
        var sql = new StringBuilder($"INSERT INTO {DbTables.Departments_Positions} (position_id, department_id) VALUES");
        var sqlValues = new string[departmentsRelationsToAdd.Length];
        for (int i = 0; i < departmentsRelationsToAdd.Length; i++)
        {
            sqlValues[i] = $" (@PositionId, @DepartmentId{i})";
            parameters.Add($"DepartmentId{i}", departmentsRelationsToAdd[i]);
        }
        sql.Append(String.Join(',', sqlValues));
        return sql.ToString();
    }

    private static string CreateDeleteSql(Guid[] departmentsRelationsToDelete, DynamicParameters parameters)
    {
        parameters.Add("ToDelete", departmentsRelationsToDelete);
        return $"DELETE FROM {DbTables.Departments_Positions} WHERE position_id = @PositionId AND department_id = ANY(@ToDelete)";
    }

    private Error HandleError(Exception ex)
    {
        _logger.LogError(ex, "Database error!");
        return Errors.Database.DatabaseError();
    }
}
