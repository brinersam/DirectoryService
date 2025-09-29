using DirectoryService.Application.Interfaces;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Shared;

namespace DirectoryService.Application.Features.Commands.QueryLocations;

public class QueryLocationsHandler
{
    private readonly ILocationRepository _repository;

    public QueryLocationsHandler(ILocationRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedData<LocationDto>> HandleAsync(QueryLocationsCommand cmd, CancellationToken ct)
    {
        var locations = await _repository.QueryLocationsAsync(
            cmd.Filters,
            cmd.Pagination,
            ct);
        return locations;
    }
}