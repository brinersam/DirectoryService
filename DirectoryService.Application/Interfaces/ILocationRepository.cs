using CSharpFunctionalExtensions;
using DirectoryService.Domain.Models.Locations;
using DirectoryService.Shared.ErrorClasses;

namespace DirectoryService.Application.Interfaces;
public interface ILocationRepository
{
    Task<UnitResult<Error>> AddLocationAsync(Location location, CancellationToken ct = default);
    Task<Result<Location, Error>> GetLocationAsync(Guid id, CancellationToken ct = default);
    Task<UnitResult<Error>> UpdateLocationAsync(Location location, CancellationToken ct = default);
}