using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models.Locations;
using DirectoryService.Domain.Models.Locations.ValueObject;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Framework;
using System.Text;
using System.Text.Json;

namespace DirectoryService.Infrastructure.Database.Repositories;
public class LocationRepository : ILocationRepository
{
    private readonly AppDb _db;

    public LocationRepository(AppDb connection)
    {
        _db = connection;
    }

    public async Task<Result<Location, Error>> GetLocationAsync(
        Guid? id = null,
        string? address = null,
        LocationName? locationName = null,
        CancellationToken ct = default)
    {
        try
        {
            var sql = new StringBuilder($"SELECT * FROM {DbTables.Locations} WHERE 1=1");
            var parameters = new DynamicParameters();

            if (id is null && address is null && locationName is null)
                throw new ArgumentNullException("All arguments are null!");

            if (id.HasValue)
            {
                sql.Append(" AND id = @Id");
                parameters.Add("Id", id.Value);
            }

            if (!string.IsNullOrEmpty(address))
            {
                sql.Append(" AND address = @Address");
                parameters.Add("Address", address);
            }

            if (locationName is not null)
            {
                var json = JsonSerializer.Serialize(locationName);
                sql.Append(" AND name = @LocationName::jsonb");
                parameters.Add("LocationName", json);
            }

            var cmd = new CommandDefinition(sql.ToString(), parameters, _db.Transaction, cancellationToken: ct);

            var result = await _db.Connection.QuerySingleOrDefaultAsync<Location>(cmd);
            if (result is null)
                return Errors.General.NotFound(typeof(Location));

            return result;
        }
        catch (Exception ex)
        {
            return Errors.Database.DatabaseError();
        }
    }

    public async Task<UnitResult<Error>> AddLocationAsync(Location location, CancellationToken ct = default)
    {
        try
        {
            var sql = @$"INSERT INTO {DbTables.Locations} 
					(id, name, address, timezone, is_active, created_at_utc, updated_at_utc) 
					VALUES 
					(@Id, @Name, @Address, @Timezone, @IsActive, @CreatedAtUtc, @UpdatedAtUtc)";

            var cmd = new CommandDefinition(sql, location, _db.Transaction, cancellationToken: ct);

            var rowsaffected = await _db.Connection.ExecuteAsync(cmd);
            if (rowsaffected <= 0)
                return Errors.Database.DBRowsAffectedError<Location>(rowsaffected, 1);

            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            return Errors.Database.DatabaseError();
        }
    }

    public async Task<UnitResult<Error>> UpdateLocationAsync(Location location, CancellationToken ct = default)
    {
        try
        {
            var sql = @$"UPDATE {DbTables.Locations} SET
					name = @Name,
					address = @Address,
					timezone = @Timezone,
					is_active = @IsActive,
					updated_at_utc = @UpdatedAtUtc
					WHERE id = @Id";

            var cmd = new CommandDefinition(sql, location, _db.Transaction, cancellationToken: ct);

            var rowsaffected = await _db.Connection.ExecuteAsync(cmd);
            if (rowsaffected <= 0)
                return Errors.Database.DBRowsAffectedError<Location>(rowsaffected, 1);

            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            return Errors.Database.DatabaseError();
        }
    }
}
