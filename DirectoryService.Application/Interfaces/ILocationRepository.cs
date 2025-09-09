using CSharpFunctionalExtensions;
using DirectoryService.Domain.Models.Locations;
using DirectoryService.Shared.ErrorClasses;

namespace DirectoryService.Application.Interfaces;
public interface ILocationRepository
{
    Task<UnitResult<Error>> AddLocationAsync(Location location);
    Task<Result<Location, Error>> GetLocationAsync(Guid id);
    Task<UnitResult<Error>> UpdateLocationAsync(Location location);
}