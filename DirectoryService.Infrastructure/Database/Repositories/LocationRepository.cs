using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models.Locations;
using DirectoryService.Shared.ErrorClasses;
using System.Data;

namespace DirectoryService.Infrastructure.Database.Repositories;
public class LocationRepository : ILocationRepository
{
    private readonly IDbConnection _connection;

    public LocationRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Result<Location, Error>> GetLocationAsync(Guid id, CancellationToken ct = default)
    {
        var sql = $"SELECT * FROM {DbTables.Locations} WHERE id = @Id";

        var cmd = new CommandDefinition(sql, new { Id = id }, cancellationToken: ct);

        var result = await _connection.QuerySingleOrDefaultAsync<Location>(cmd);
        if (result is null)
            return Errors.General.NotFound(typeof(Location));

        return result;
    }

    public async Task<UnitResult<Error>> AddLocationAsync(Location location, CancellationToken ct = default)
    {
        var validationSql = $@"SELECT name FROM {DbTables.Locations} WHERE name = @Name";
        var validationCmd = new CommandDefinition(validationSql, new {Name = location.Name}, cancellationToken: ct);
        var existingRecord = await _connection.QuerySingleOrDefaultAsync<Location>(validationCmd);
        if (existingRecord is not null)
            return Error.Failure($"Can not add duplicate location with name [{location.Name.Value}]");

        var sql = @$"INSERT INTO {DbTables.Locations} 
					(id, name, address, timezone, is_active, created_at_utc, updated_at_utc) 
					VALUES 
					(@Id, @Name, @Address, @Timezone, @IsActive, @CreatedAtUtc, @UpdatedAtUtc)";

        var cmd = new CommandDefinition(sql, location, cancellationToken: ct);

        var rowsaffected = await _connection.ExecuteAsync(cmd);
        if (rowsaffected <= 0)
            return Errors.General.DBRowsAffectedError<Location>(rowsaffected, 1);

        return Result.Success<Error>();
    }

    public async Task<UnitResult<Error>> UpdateLocationAsync(Location location, CancellationToken ct = default)
    {
        var sql = @$"UPDATE {DbTables.Locations} SET
					name = @Name,
					address = @Address,
					timezone = @Timezone,
					is_active = @IsActive,
					updated_at_utc = @UpdatedAtUtc
					WHERE id = @Id";

        var cmd = new CommandDefinition(sql, location, cancellationToken: ct);

        var rowsaffected = await _connection.ExecuteAsync(cmd);
        if (rowsaffected <= 0)
            return Errors.General.DBRowsAffectedError<Location>(rowsaffected, 1);

        return Result.Success<Error>();
    }
}
