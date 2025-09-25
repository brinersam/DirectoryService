
namespace DirectoryService.Presentation.Controllers;

public record SetDepartmentParentCommand(
    Guid departmentId,
    SetDepartmentParentRequest request);
