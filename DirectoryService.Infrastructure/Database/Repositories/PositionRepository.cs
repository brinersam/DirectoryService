using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Domain.Models;
using DirectoryService.Shared.ErrorClasses;
using System.Data;

namespace DirectoryService.Infrastructure.Database.Repositories;
public class PositionRepository
{
    private readonly IDbConnection _connection;

    public PositionRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Result<Position, Error>> GetPositionAsync(Guid id)
    {
        var sql = $"SELECT * FROM {DbTables.Positions} WHERE Id = @Id";

        var result = await _connection.QuerySingleOrDefaultAsync<Position>(sql, new { Id = id });
        if (result is null)
            return Errors.General.NotFound(typeof(Position));

        return result;
    }

    public async Task<UnitResult<Error>> AddPositionAsync(Position position)
    {
        var sql = @$"INSERT INTO {DbTables.Positions} 
                    (Id, Name, Description, IsActive, CreatedAtUtc, UpdatedAtUtc) 
                    VALUES 
                    (@Id, @Name, @Description, @IsActive, @CreatedAtUtc, @UpdatedAtUtc)";

        var rowsaffected = await _connection.ExecuteAsync(sql, position);
        if (rowsaffected <= 0)
            return Errors.General.DBRowsAffectedError<Position>(rowsaffected, 1);

        return Result.Success<Error>();
    }

    public async Task<UnitResult<Error>> UpdatePositionAsync(Position position)
    {
        var sql = @$"UPDATE {DbTables.Positions} SET
                    Name = @Name,
                    Description = @Description,
                    IsActive = @IsActive,
                    UpdatedAtUtc = @UpdatedAtUtc
                    WHERE Id = @Id";

        var rowsaffected = await _connection.ExecuteAsync(sql, position);
        if (rowsaffected <= 0)
            return Errors.General.DBRowsAffectedError<Position>(rowsaffected, 1);

        return Result.Success<Error>();
    }
}
