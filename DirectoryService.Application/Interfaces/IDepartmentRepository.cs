using CSharpFunctionalExtensions;
using DirectoryService.Domain.Models.Departments;
using DirectoryService.Shared.ErrorClasses;

namespace DirectoryService.Application.Interfaces;
public interface IDepartmentRepository
{
    Task<UnitResult<Error>> AddDepartmentAsync(Department department, CancellationToken ct = default);

    Task<UnitResult<Error>> MoveDepartmentToParent(Guid departmentId, Guid? parentId, CancellationToken ct = default);

    Task<Department> GetDepartmentAsync(Guid? id, bool? isActive = true, CancellationToken ct = default);

    Task<Result<List<Department>, Error>> GetDepartmentsAsync(IEnumerable<Guid> ids, bool active = true, CancellationToken ct = default);

    Task<UnitResult<Error>> UpdateDepartmentAsync(Department department, CancellationToken ct = default);
}