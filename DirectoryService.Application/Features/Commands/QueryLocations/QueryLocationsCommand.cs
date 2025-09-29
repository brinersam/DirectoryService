using DirectoryService.Shared;
using DirectoryService.Shared.SharedDtos.Locations;

namespace DirectoryService.Application.Features.Commands.QueryLocations;

public record QueryLocationsCommand(LocationFilters Filters, PaginationDto? Pagination);