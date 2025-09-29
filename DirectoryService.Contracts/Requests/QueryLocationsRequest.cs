using DirectoryService.Shared;

namespace DirectoryService.Contracts.Requests;

public record QueryLocationsRequest(
    Guid[]? DepartmentIds,
    string? SearchName,
    bool? IsActive,
    PaginationDto? Pagination);
