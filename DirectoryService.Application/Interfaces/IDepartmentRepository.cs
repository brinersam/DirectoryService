using CSharpFunctionalExtensions;
using DirectoryService.Domain.Models.Departments;
using DirectoryService.Shared.ErrorClasses;

namespace DirectoryService.Application.Interfaces;
public interface IDepartmentRepository
{
    Task<UnitResult<Error>> AddDepartmentAsync(Department department);

    Task<Result<Department, Error>> GetDepartmentAsync(Guid id);

    Task<Result<List<Department>, Error>> GetDepartmentsAsync(IEnumerable<Guid> ids, bool active = true, CancellationToken ct = default);

    Task<UnitResult<Error>> UpdateDepartmentAsync(Department department);
}