using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models;
using DirectoryService.Infrastructure.Database.Datamodels;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Framework;
using System.Data;
using System.Data.Common;
using System.Text;

namespace DirectoryService.Infrastructure.Database.Repositories;
public class PositionRepository : IPositionRepository
{
    private readonly AppDb _db;

    public PositionRepository(AppDb connection)
    {
        _db = connection;
    }

    public async Task<Result<Position, Error>> GetPositionAsync(
        Guid? id = null,
        string name = "",
        bool active = true,
        CancellationToken ct = default)
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

        var result = await _db.Connection.QuerySingleOrDefaultAsync<Position>(sql, new { Id = id });
        if (result is null)
            return Errors.General.NotFound(typeof(Position));

        return result;
    }

    public async Task<IEnumerable<Guid>> GetRelatedDepartmentIds(Guid positionId, CancellationToken ct = default)
    {
        var guidsql = $"SELECT department_id FROM {DbTables.Departments_Positions} WHERE position_id = @Id";
        var cmd = new CommandDefinition(guidsql, new { Id = positionId }, transaction: _db.Transaction, cancellationToken: ct);
        return (await _db.Connection.QueryAsync<Guid>(cmd)).ToList();
    }

    public async Task<UnitResult<Error>> AddPositionAsync(Position position, CancellationToken ct = default)
    {
        var sql = @$"INSERT INTO {DbTables.Positions} 
					(id, name, description, is_active, created_at_utc, updated_at_utc) 
					VALUES 
					(@Id, @Name, @Description, @IsActive, @CreatedAtUtc, @UpdatedAtUtc)";

        var cmd = new CommandDefinition(sql, position, _db.Transaction, cancellationToken: ct);


        var rowsaffected = await _db.Connection.ExecuteAsync(cmd);
        if (rowsaffected <= 0)
            return Errors.General.DBRowsAffectedError<Position>(rowsaffected, 1);



        return Result.Success<Error>();
    }

    public async Task<UnitResult<Error>> UpdatePositionAsync(Position position, CancellationToken ct = default)
    {
        var sql = @$"UPDATE {DbTables.Positions} SET
					name = @Name,
					description = @Description,
					is_active = @IsActive,
					updated_at_utc = @UpdatedAtUtc
					WHERE id = @Id";


        var cmd = new CommandDefinition(sql, position, _db.Transaction, cancellationToken: ct);

        var rowsaffected = await _db.Connection.ExecuteAsync(cmd);
        if (rowsaffected <= 0)
            return Errors.General.DBRowsAffectedError<Position>(rowsaffected, 1);


        return Result.Success<Error>();
    }
}
