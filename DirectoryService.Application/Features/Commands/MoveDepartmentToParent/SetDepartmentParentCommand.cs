using DirectoryService.Contracts.Requests;

namespace DirectoryService.Application.Features.Commands.MoveDepartmentToParent;

public record SetDepartmentParentCommand(
    Guid departmentId,
    SetDepartmentParentRequest request);
