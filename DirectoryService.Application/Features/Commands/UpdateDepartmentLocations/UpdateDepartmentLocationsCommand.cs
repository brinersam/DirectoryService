using DirectoryService.Contracts.Requests;

namespace DirectoryService.Application.Features.Commands.UpdateDepartmentLocations;

public record UpdateDepartmentLocationsCommand(
    Guid DepartmentId,
    UpdateDepartmentLocationsRequest Request);