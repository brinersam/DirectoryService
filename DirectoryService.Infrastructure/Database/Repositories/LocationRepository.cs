using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Domain.Models.Locations;
using DirectoryService.Shared.ErrorClasses;
using System.Data;

namespace DirectoryService.Infrastructure.Database.Repositories;
public class LocationRepository
{
    private readonly IDbConnection _connection;

    public LocationRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Result<Location, Error>> GetLocationAsync(Guid id)
    {
        var sql = $"SELECT * FROM {DbTables.Locations} WHERE Id = @Id";

        var result = await _connection.QuerySingleOrDefaultAsync<Location>(sql, new { Id = id });
        if (result is null)
            return Errors.General.NotFound(typeof(Location));

        return result;
    }

    public async Task<UnitResult<Error>> AddLocationAsync(Location location)
    {
        var sql = @$"INSERT INTO {DbTables.Locations} 
                    (Id, Name, Address, Timezone, IsActive, CreatedAt, UpdatedAt) 
                    VALUES 
                    (@Id, @Name, @Address, @Timezone, @IsActive, @CreatedAt, @UpdatedAt)";

        var rowsaffected = await _connection.ExecuteAsync(sql, location);
        if (rowsaffected <= 0)
            return Errors.General.DBRowsAffectedError<Location>(rowsaffected, 1);

        return Result.Success<Error>();
    }

    public async Task<UnitResult<Error>> UpdateLocationAsync(Location location)
    {
        var sql = @$"UPDATE {DbTables.Locations} SET
                    Name = @Name,
                    Address = @Address,
                    Timezone = @Timezone,
                    IsActive = @IsActive,
                    UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";

        var rowsaffected = await _connection.ExecuteAsync(sql, location);
        if (rowsaffected <= 0)
            return Errors.General.DBRowsAffectedError<Location>(rowsaffected, 1);

        return Result.Success<Error>();
    }
}
