namespace DirectoryService.Contracts.Requests;

public record CreatePositionRequest(
    string Name,
    string Description,
    Guid[] DepartmentIds);
