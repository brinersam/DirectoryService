using DirectoryService.Shared;

namespace DirectoryService.Presentation.Controllers;

public record GetLocationsRequest(
    Guid[]? DepartmentIds,
    string? SearchName,
    bool? IsActive,
    PaginationDto? Pagination);
