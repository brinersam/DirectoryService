using CSharpFunctionalExtensions;
using DirectoryService.Domain.Models.Locations;
using DirectoryService.Domain.Models.Locations.ValueObject;
using DirectoryService.Shared.ErrorClasses;

namespace DirectoryService.Application.Interfaces;
public interface ILocationRepository
{
    Task<UnitResult<Error>> AddLocationAsync(Location location, CancellationToken ct = default);
    Task<Result<Location, Error>> GetLocationAsync(Guid? id = null, string? address = null, LocationName? locationName = null, CancellationToken ct = default);
    Task<UnitResult<Error>> UpdateLocationAsync(Location location, CancellationToken ct = default);
}