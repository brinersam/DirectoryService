using DirectoryService.Application.Interfaces;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Shared;

namespace DirectoryService.Presentation.Controllers;

public class GetLocationsHandler
{
    private readonly ILocationRepository _repository;

    public GetLocationsHandler(ILocationRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedData<LocationDto>> HandleAsync(GetLocationsCommand cmd, CancellationToken ct)
    {
        var locations = await _repository.QueryLocationsAsync(
            cmd.Filters,
            cmd.Pagination,
            ct);
        return locations;
    }
}