namespace DirectoryService.Contracts.Requests;

public record CreateDepartmentRequest(
    string Name,
    string Identifier,
    Guid? ParentId,
    Guid[] LocationsId);
