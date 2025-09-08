using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models.Departments;
using DirectoryService.Shared.ErrorClasses;
using System.Data;

namespace DirectoryService.Infrastructure.Database.Repositories;
public class DepartmentRepository : IDepartmentRepository
{
    private readonly IDbConnection _connection;

    public DepartmentRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Result<Department, Error>> GetDepartmentAsync(Guid id)
    {
        var sql = $"SELECT * FROM {DbTables.Departments} WHERE id = @Id";

        var result = await _connection.QuerySingleOrDefaultAsync<Department>(sql, new { Id = id });
        if (result is null)
            return Errors.General.NotFound(typeof(Department));

        return result;
    }

    public async Task<UnitResult<Error>> AddDepartmentAsync(Department department)
    {
        var sql = @$"INSERT INTO departments
					(id, name, identifier, parent_id, path, depth, is_active, created_at_utc, updated_at_utc) 
					VALUES 
					(@Id, @Name, @Identifier, @ParentId, @Path, @Depth, @IsActive, @CreatedAtUtc, @UpdatedAtUtc)";

        var rowsaffected = await _connection.ExecuteAsync(sql, department);
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

        var rowsaffected = await _connection.ExecuteAsync(sql, department);
        if (rowsaffected <= 0)
            return Errors.General.DBRowsAffectedError<Department>(rowsaffected, 1);

        return Result.Success<Error>();
    }
}
