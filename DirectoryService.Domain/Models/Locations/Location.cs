using CSharpFunctionalExtensions;
using DirectoryService.Domain.Models.Locations.ValueObject;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Validator;
using System.Text.Json.Serialization;

namespace DirectoryService.Domain.Models.Locations;
public class Location
{
    public Guid Id { get; private set; }

    public LocationName Name { get; private set; }

    public string Address { get; private set; }

    public string Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    [JsonConstructor]
    private Location() { }

    private Location(Guid id, LocationName name, string address, string timezone, bool isActive, DateTime createdAtUtc, DateTime updatedAtUtc)
    {
        Id = id;
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public static Result<Location, List<Error>> Create(LocationName name, string address, string timezone)
    {
        var validator = new ModelValidator();

        validator.Validate(timezone)
            .HasFormat(FormatRulesEnum.IANA);

        var errors = validator.ValidateAll(out bool isError);
        if (isError)
            return errors.SelectMany(x => x).ToList();

        var now = DateTime.UtcNow;

        return new Location(
            Guid.NewGuid(),
            name,
            address,
            timezone,
            isActive: true,
            createdAtUtc: now,
            updatedAtUtc: now
        );
    }
}
