using CSharpFunctionalExtensions;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models.Locations;
using DirectoryService.Domain.Models.Locations.ValueObject;
using DirectoryService.Shared.ErrorClasses;

namespace DirectoryService.Application.Features.Commands.CreateLocation;
public class CreateLocationHandler
{
    private readonly ILocationRepository _repository;

    public CreateLocationHandler(ILocationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid, List<Error>>> HandleAsync(
        CreateLocationCommand cmd,
        CancellationToken ct = default)
    {
        var createLocationNameRes = LocationName.Create(cmd.LocationName);
        if (createLocationNameRes.IsFailure)
            return createLocationNameRes.Error;

        var createLocation = Location.Create(
            createLocationNameRes.Value,
            cmd.Address,
            cmd.Timezone);
        if (createLocation.IsFailure)
            return createLocation.Error;

        var result = await _repository.AddLocationAsync(createLocation.Value, ct);
        if (result.IsFailure)
            return new List<Error>(){result.Error};

        return createLocation.Value.Id;
    }
}
