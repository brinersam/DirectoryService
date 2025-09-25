using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models;
using DirectoryService.Domain.Models.Departments;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Framework.DbConnection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DirectoryService.Infrastructure.Database.Repositories;
public class DepartmentRepository : IDepartmentRepository
{
    private readonly ILogger<DepartmentRepository> _logger;
    private readonly AppDb _db;

    public DepartmentRepository(
        ILogger<DepartmentRepository> logger,
        AppDb connection)
    {
        _logger = logger;
        _db = connection;
    }

    public async Task<Department> GetDepartmentAsync(Guid? id, bool? isActive = true, CancellationToken ct = default)
    {
        try
        {
            var sql = new StringBuilder($"SELECT * FROM {DbTables.Departments} WHERE id = @Id");
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            if (isActive.HasValue)
            {
                parameters.Add("IsActive", isActive);
                sql.Append(" AND is_active = @IsActive");
            }

            var cmd = new CommandDefinition(sql.ToString(), parameters, _db.Transaction, cancellationToken: ct);

            var dataModel = await _db.Connection.QuerySingleOrDefaultAsync<Department>(cmd);
            if (dataModel is null)
                return dataModel!;

            var locationIds = await GetRelatedLocationIds(dataModel.Id, ct);
            if (locationIds.IsFailure)
                return dataModel;

            var result = new Department(
                dataModel.Id,
                dataModel.Name,
                dataModel.Identifier,
                dataModel.ParentId,
                dataModel.Path,
                dataModel.Depth,
                locationIds.Value);

            return result;
        }
        catch (Exception ex)
        {
            HandleError(ex);
            return null!;
        }
    }

    public async Task<Result<List<Department>, Error>> GetDepartmentsAsync(IEnumerable<Guid> ids, bool active = true, CancellationToken ct = default)
    {
        try
        {
            var sql = new StringBuilder($"SELECT * FROM {DbTables.Departments} WHERE is_active = @Active AND id = ANY(@Ids)");

            var cmd = new CommandDefinition(sql.ToString(), new { Ids = ids.ToArray(), Active = active }, _db.Transaction, cancellationToken: ct);

            var result = await _db.Connection.QueryAsync<Department>(cmd);
            if (result.Any() == false)
                return Errors.General.NotFound(typeof(Department));

            return result.ToList();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    public async Task<UnitResult<Error>> AddDepartmentAsync(Department department, CancellationToken ct = default)
    {
        try
        {
            var sql = @$"INSERT INTO departments
					(id, name, identifier, parent_id, path, depth, is_active, created_at_utc, updated_at_utc) 
					VALUES 
					(@Id, @Name, @Identifier, @ParentId, @Path, @Depth, @IsActive, @CreatedAtUtc, @UpdatedAtUtc)";

            using var transaction = _db.OpenTransactionIfNotOngoing().Value;

            var cmd = new CommandDefinition(sql, department, _db.Transaction, cancellationToken: ct);

            var rowsaffected = await _db.Connection.ExecuteAsync(sql, department);
            if (rowsaffected <= 0)
                return Errors.Database.DBRowsAffectedError<Department>(rowsaffected, 1);

            var syncRes = await SyncDepartmentWithLocations(department, ct);
            if (syncRes.IsFailure)
                return syncRes.Error;

            transaction.Commit();

            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    public async Task<UnitResult<Error>> UpdateDepartmentAsync(Department department, CancellationToken ct = default)
    {
        try
        {
            var sql = $@"UPDATE {DbTables.Departments} SET
				name = @Name,
				identifier = @Identifier,
				parent_id = @ParentId,
				path = @Path,
				depth = @Depth,
				is_active = @IsActive,
				updated_at_utc = @UpdatedAtUtc
			WHERE id = @Id";

            using var transaction = _db.OpenTransactionIfNotOngoing().Value;

            var cmd = new CommandDefinition(sql, department, _db.Transaction, cancellationToken: ct);

            var rowsaffected = await _db.Connection.ExecuteAsync(sql, department);
            if (rowsaffected <= 0)
                return Errors.Database.DBRowsAffectedError<Department>(rowsaffected, 1);

            var syncRes = await SyncDepartmentWithLocations(department, ct);
            if (syncRes.IsFailure)
                return syncRes.Error;

            transaction.Commit();

            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    public async Task<Result<IEnumerable<Guid>, Error>> GetRelatedLocationIds(Guid departmentId, CancellationToken ct = default)
    {
        try
        {
            var guidsql = $"SELECT location_id FROM {DbTables.Department_Location} WHERE department_id = @Id";
            var cmd = new CommandDefinition(guidsql, new { Id = departmentId }, transaction: _db.Transaction, cancellationToken: ct);
            return (await _db.Connection.QueryAsync<Guid>(cmd)).ToList();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    public async Task<UnitResult<Error>> SyncDepartmentWithLocations(Department department, CancellationToken ct = default)
    {
        try
        {
            var dbLocationIdsRes = await GetRelatedLocationIds(department.Id, ct);
            if (dbLocationIdsRes.IsFailure)
                return dbLocationIdsRes.Error;

            var locationRelationsToDelete = dbLocationIdsRes.Value.Where(x => department.LocationIds.Contains(x) is false).ToArray();
            var locationRelationsToAdd = department.LocationIds.Where(x => dbLocationIdsRes.Value.Contains(x) is false).ToArray();

            if (!locationRelationsToAdd.Any() && !locationRelationsToDelete.Any())
                return Result.Success<Error>();

            var parameters = new DynamicParameters();
            parameters.Add("DepartmentId", department.Id);

            var sql = new StringBuilder();

            var sqlCmds = new List<string>();
            if (locationRelationsToDelete.Any())
                sqlCmds.Add(CreateDeleteSql(locationRelationsToDelete, parameters));

            if (locationRelationsToAdd.Any())
                sqlCmds.Add(CreateInsertSql(locationRelationsToAdd, parameters));

            sql.Append(String.Join(';', sqlCmds));
            sql.Append(";");

            var cmd = new CommandDefinition(sql.ToString(), parameters, transaction: _db.Transaction, cancellationToken: ct);

            var rowsAffected = await _db.Connection.ExecuteAsync(cmd);
            var expectedAffected = locationRelationsToDelete.Length + locationRelationsToAdd.Length;
            if (rowsAffected != expectedAffected)
                return Errors.Database.DBRowsAffectedError<Position>(rowsAffected, expectedAffected);

            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    public async Task<UnitResult<Error>> MoveDepartmentToParent(
        Guid departmentId,
        Guid? parentId,
        CancellationToken ct = default)
    {
        try
        {
            using var transaction = _db.OpenTransactionIfNotOngoing().Value;

            var lockedChildDepartments = await GetChildrenIdsOf(departmentId, @lock: true, ct); // also locks department being moved

            if (parentId.HasValue)
            {
                var departmentInChildren = lockedChildDepartments.Contains((Guid)parentId);
                if (departmentInChildren)
                    return Error.Failure("Can not move department into its child");
            }

            var mutationSql = $"subpath(d.path, nlevel(moving_element.path)-1)";
            if (parentId.HasValue)
                mutationSql = $"parent.path || subpath(d.path, nlevel(moving_element.path)-1)";

            var sql =
            $"""
                UPDATE {DbTables.Departments} --update the id
                SET 
                    parent_id = @ParentId
                WHERE
                    id = @ChildId;

                WITH new_parent AS --update the paths, depths
                (
                    SELECT id, path
                    FROM {DbTables.Departments} 
                    WHERE {DbTables.Departments}.id = @ParentId
                ),
                moving_element AS 
                (
                    SELECT path
                    FROM {DbTables.Departments}
                    WHERE {DbTables.Departments}.id = @ChildId
                )
                UPDATE {DbTables.Departments} d
                SET 
                    path = {mutationSql},
                    depth = nlevel({mutationSql})-1
                FROM
                    moving_element
                LEFT JOIN new_parent parent ON true --in case of null parent since FROM is implicit inner join which also breaks moving_element
                WHERE 
                    d.path <@ moving_element.path;
            """;

            var parameters = new DynamicParameters();
            parameters.Add("ChildId", departmentId);
            parameters.Add("ParentId", parentId, dbType: System.Data.DbType.Guid);

            var cmd = new CommandDefinition(sql, parameters, transaction: _db.Transaction, cancellationToken: ct);
            await _db.Connection.ExecuteAsync(cmd);

            transaction.Commit();

            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    private async Task<HashSet<Guid>> GetChildrenIdsOf(Guid parentDepartmentId, bool @lock = false, CancellationToken ct = default)
    {
        var sql = new StringBuilder(
                $"""
                    WITH dep AS --lock the tree
                    (
                        SELECT path
                        FROM {DbTables.Departments}
                        WHERE {DbTables.Departments}.id = @DepId
                    )
                    SELECT children.id
                    FROM 
                        {DbTables.Departments} children,
                        dep
                    WHERE children.path <@ dep.path
                """);

        if (@lock)
            sql.Append(" FOR UPDATE");
        sql.Append(";");

        var cmdChildren = new CommandDefinition(sql.ToString(), new { DepId = parentDepartmentId }, transaction: _db.Transaction, cancellationToken: ct);
        return (await _db.Connection.QueryAsync<Guid>(cmdChildren)).Where(x => x != parentDepartmentId).ToHashSet();
    }

    private static string CreateInsertSql(Guid[] locationRelationsToAdd, DynamicParameters parameters)
    {
        var sql = new StringBuilder($"INSERT INTO {DbTables.Department_Location} (department_id, location_id) VALUES");
        var sqlValues = new string[locationRelationsToAdd.Length];
        for (int i = 0; i < locationRelationsToAdd.Length; i++)
        {
            sqlValues[i] = $" (@DepartmentId, @LocationId{i})";
            parameters.Add($"LocationId{i}", locationRelationsToAdd[i]);
        }
        sql.Append(String.Join(',', sqlValues));
        return sql.ToString();
    }

    private static string CreateDeleteSql(Guid[] locationRelationsToDelete, DynamicParameters parameters)
    {
        parameters.Add("ToDelete", locationRelationsToDelete);
        return $"DELETE FROM {DbTables.Department_Location} WHERE department_id = @DepartmentId AND location_id = ANY(@ToDelete)";
    }

    private Error HandleError(Exception ex)
    {
        _logger.LogError(ex, "Database error!");
        return Errors.Database.DatabaseError();
    }
}
