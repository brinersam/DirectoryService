using CSharpFunctionalExtensions;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.ModelInterfaces;
using DirectoryService.Shared.Validator;

namespace DirectoryService.Domain.Models.Locations.ValueObject;
public class LocationName : IJsonbObject
{
    public string Value { get; init; }

    private LocationName(string value)
    {
        Value = value;
    }

    public static Result<LocationName, List<Error>> Create(string value)
    {
        var validator = new ModelValidator();

        validator.Validate(value)
            .MinLength(3)
            .MaxLength(120);

        var errors = validator.ValidateAll(out bool isError);
        if (isError)
            return errors.SelectMany(x => x).ToList();

        return new LocationName(value);
    }
}
