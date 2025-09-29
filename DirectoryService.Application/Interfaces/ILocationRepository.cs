using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Domain.Models.Locations;
using DirectoryService.Domain.Models.Locations.ValueObject;
using DirectoryService.Shared;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.SharedDtos.Locations;

namespace DirectoryService.Application.Interfaces;
public interface ILocationRepository
{
    Task<UnitResult<Error>> AddLocationAsync(Location location, CancellationToken ct = default);

    Task<Result<Location, Error>> GetLocationAsync(Guid? id = null, string? address = null, LocationName? locationName = null, CancellationToken ct = default);

    Task<PaginatedData<LocationDto>> QueryLocationsAsync(LocationFilters? filters, PaginationDto? pagination, CancellationToken ct = default);

    Task<List<Location>> GetLocationsAsync(Guid[] locationIds, bool active = true, CancellationToken ct = default);

    Task<UnitResult<Error>> UpdateLocationAsync(Location location, CancellationToken ct = default);
}