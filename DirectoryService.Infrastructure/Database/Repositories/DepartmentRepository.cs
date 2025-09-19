using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models.Departments;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Framework;
using System.Text;

namespace DirectoryService.Infrastructure.Database.Repositories;
public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDb _db;

    public DepartmentRepository(AppDb connection)
    {
        _db = connection;
    }

    public async Task<Result<Department, Error>> GetDepartmentAsync(Guid id)
    {
        var sql = $"SELECT * FROM {DbTables.Departments} WHERE id = @Id";

        var result = await _db.Connection.QuerySingleOrDefaultAsync<Department>(sql, new { Id = id });
        if (result is null)
            return Errors.General.NotFound(typeof(Department));

        return result;
    }

    public async Task<Result<List<Department>, Error>> GetDepartmentsAsync(IEnumerable<Guid> ids, bool active = true, CancellationToken ct = default)
    {
        var sql = new StringBuilder($"SELECT * FROM {DbTables.Departments} WHERE is_active = @Active AND id = ANY(@Ids)");

        var cmd = new CommandDefinition(sql.ToString(), new { Ids = ids.ToArray(), Active = active }, _db.Transaction, cancellationToken: ct);

        var result = await _db.Connection.QueryAsync<Department>(cmd);
        if (result.Any() == false)
            return Errors.General.NotFound(typeof(Department));

        return result.ToList();
    }

    public async Task<UnitResult<Error>> AddDepartmentAsync(Department department)
    {
        var sql = @$"INSERT INTO departments
					(id, name, identifier, parent_id, path, depth, is_active, created_at_utc, updated_at_utc) 
					VALUES 
					(@Id, @Name, @Identifier, @ParentId, @Path, @Depth, @IsActive, @CreatedAtUtc, @UpdatedAtUtc)";

        var rowsaffected = await _db.Connection.ExecuteAsync(sql, department);
        if (rowsaffected <= 0)
            return Errors.General.DBRowsAffectedError<Department>(rowsaffected, 1);

        return Result.Success<Error>();
    }

    public async Task<UnitResult<Error>> UpdateDepartmentAsync(Department department)
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

        var rowsaffected = await _db.Connection.ExecuteAsync(sql, department);
        if (rowsaffected <= 0)
            return Errors.General.DBRowsAffectedError<Department>(rowsaffected, 1);

        return Result.Success<Error>();
    }
}
