using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Interfaces;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Domain.Models.Locations;
using DirectoryService.Domain.Models.Locations.ValueObject;
using DirectoryService.Shared;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Framework.DbConnection;
using DirectoryService.Shared.SharedDtos.Locations;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace DirectoryService.Infrastructure.Database.Repositories;
public class LocationRepository : ILocationRepository
{
    private readonly ILogger<LocationRepository> _logger;
    private readonly AppDb _db;

    public LocationRepository(
        ILogger<LocationRepository> logger,
        AppDb connection)
    {
        _logger = logger;
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
            return HandleError(ex);
        }
    }

    public async Task<PaginatedData<LocationDto>> QueryLocationsAsync(
        LocationFilters? filters,
        PaginationDto? pagination,
        CancellationToken ct)
    {
        try
        {
            pagination ??= new PaginationDto();

            var parameters = new DynamicParameters();
            var cteSql = String.Empty;
            var joinSql = String.Empty;
            var whereSql = new StringBuilder("WHERE 1=1");

            var limitOffsetSql = "LIMIT @Take OFFSET @Skip";
            parameters.Add("Take", pagination.Take);
            parameters.Add("Skip", pagination.Take * (pagination.Page - 1));

            if (filters is not null)
                ApplyFilters_QueryLocationsAsync(filters, parameters, ref cteSql, ref joinSql, whereSql);

            var sql = $"""
                {cteSql}
                SELECT 
                    id,
                    name->>'Value' as location_name,
                    address,
                    timezone,
                    is_active,
                    created_at_utc,
                    updated_at_utc,
                    COUNT (*) OVER() as total_count
                FROM {DbTables.Locations} l
                {joinSql}
                {whereSql} 
                ORDER BY name ASC, created_at_utc DESC
                {limitOffsetSql} ;
                """;

            var cmd = new CommandDefinition(sql, parameters, _db.Transaction, cancellationToken: ct);

            long? totalElements = null;
            var result = await _db.Connection.QueryAsync<LocationDto, long, LocationDto>(
                cmd,
                splitOn: "total_count",
                map: (dto, count) =>
                {
                    totalElements ??= count;
                    return dto;
                });

            return result.ToPaginatedList(pagination.Page, totalElements);
        }
        catch (Exception ex)
        {
            HandleError(ex);
            return new List<LocationDto>().ToPaginatedList(0, 0);
        }
    }

    private static void ApplyFilters_QueryLocationsAsync(LocationFilters filters, DynamicParameters parameters, ref string cteSql, ref string joinSql, StringBuilder whereSql)
    {
        foreach (var (paramName, value) in filters.IterateOnNonNullValues())
        {
            switch (paramName)
            {
                case nameof(LocationFilters.DepartmentIds):
                    {
                        cteSql = $"""
                                    WITH related_departments AS
                                    (
                                        SELECT DISTINCT location_id
                                        FROM department_location dl
                                        WHERE 
                                        department_id = ANY(@{paramName})
                                    )
                                    """;
                        joinSql = "INNER JOIN related_departments rd ON rd.location_id = l.id";

                        parameters.Add(paramName, value);
                        break;
                    }

                case nameof(LocationFilters.SearchName):
                    {
                        whereSql.Append($" AND name->>'Value' ILIKE '%' || @{paramName} || '%'");
                        parameters.Add(paramName, value);
                        break;
                    }

                case nameof(LocationFilters.IsActive):
                    {
                        whereSql.Append($" AND is_active = @{paramName}");
                        parameters.Add(paramName, value);
                        break;
                    }
            }
        }
    }

    public async Task<List<Location>> GetLocationsAsync(Guid[] locationIds, bool active = true, CancellationToken ct = default)
    {
        try
        {
            var sql = new StringBuilder($"SELECT * FROM {DbTables.Locations} WHERE is_active = @Active AND id = ANY(@Ids)");

            var cmd = new CommandDefinition(sql.ToString(), new { Ids = locationIds.ToArray(), Active = active }, _db.Transaction, cancellationToken: ct);

            var result = await _db.Connection.QueryAsync<Location>(cmd);
            return result.ToList();
        }
        catch (Exception ex)
        {
            HandleError(ex);
            return new List<Location> { };
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
            return HandleError(ex);
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
            return HandleError(ex);
        }
    }

    private Error HandleError(Exception ex)
    {
        _logger.LogError(ex, "Database error!");
        return Errors.Database.DatabaseError();
    }
}
