using CSharpFunctionalExtensions;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models.Locations;
using DirectoryService.Domain.Models.Locations.ValueObject;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Validator;
using FluentValidation;

namespace DirectoryService.Application.Features.Commands.CreateLocation;
public class CreateLocationHandler
{
    private readonly IValidator<CreateLocationCommand> _cmdValidator;
    private readonly ILocationRepository _repository;

    public CreateLocationHandler(
        IValidator<CreateLocationCommand> cmdValidator,
        ILocationRepository repository)
    {
        _cmdValidator = cmdValidator;
        _repository = repository;
    }

    public async Task<Result<Guid, IEnumerable<Error>>> HandleAsync(
        CreateLocationCommand cmd,
        CancellationToken ct = default)
    {
        var cmdValidation = _cmdValidator.Validate(cmd);
        if (cmdValidation.IsValid == false)
            return cmdValidation.Errors.ToAppErrors().ToList();

        var createLocationNameRes = LocationName.Create(cmd.LocationName);
        if (createLocationNameRes.IsFailure)
            return createLocationNameRes.Error;

        var createLocation = Location.Create(
            createLocationNameRes.Value,
            cmd.Address,
            cmd.Timezone);
        if (createLocation.IsFailure)
            return createLocation.Error;

        var existingLocation = await _repository.GetLocationAsync(
            address: createLocation.Value.Address,
            locationName: createLocation.Value.Name,
            ct: ct);

        if (existingLocation.IsSuccess)
            return new[] { Error.Failure($"Can not add duplicate location with name [{createLocation.Value.Name.Value}]") };

        var result = await _repository.AddLocationAsync(createLocation.Value, ct);
        if (result.IsFailure)
            return new[] { result.Error };

        return createLocation.Value.Id;
    }
}
