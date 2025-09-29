using DirectoryService.Shared;
using DirectoryService.Shared.SharedDtos.Locations;

namespace DirectoryService.Presentation.Controllers;

public record GetLocationsCommand(LocationFilters Filters, PaginationDto? Pagination);