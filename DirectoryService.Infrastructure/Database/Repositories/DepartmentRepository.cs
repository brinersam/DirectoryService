using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Domain.Models.Departments;
using DirectoryService.Shared.ErrorClasses;
using System.Data;

namespace DirectoryService.Infrastructure.Database.Repositories;
public class DepartmentRepository
{
    private readonly IDbConnection _connection;

    public DepartmentRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Result<Department, Error>> GetDepartmentAsync(Guid id)
    {
        var sql = $"SELECT * FROM {DbTables.Departments} WHERE Id = @Id";

        var result = await _connection.QuerySingleOrDefaultAsync<Department>(sql, new { Id = id });
        if (result is null)
            return Errors.General.NotFound(typeof(Department));

        return result;
    }

    public async Task<UnitResult<Error>> AddDepartmentAsync(Department department)
    {
        var sql = @$"INSERT INTO {DbTables.Departments} 
                    (Id, Name, Identifier, ParentId, Path, Depth, IsActive, CreatedAtUtc, UpdatedAtUtc) 
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
                Name = @Name,
                Identifier = @Identifier,
                ParentId = @ParentId,
                Path = @Path,
                Depth = @Depth,
                IsActive = @IsActive,
                UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @Id";

        var rowsaffected = await _connection.ExecuteAsync(sql, department);
        if (rowsaffected <= 0)
            return Errors.General.DBRowsAffectedError<Department>(rowsaffected, 1);

        return Result.Success<Error>();
    }
}
