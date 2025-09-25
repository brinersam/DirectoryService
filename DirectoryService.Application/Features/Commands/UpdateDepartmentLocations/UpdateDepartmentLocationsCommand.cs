using DirectoryService.Contracts.Requests;

namespace DirectoryService.Application.Features.Commands.UpdateDepartment;

public record UpdateDepartmentLocationsCommand(
    Guid DepartmentId,
    UpdateDepartmentLocationsRequest Request);