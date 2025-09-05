using CSharpFunctionalExtensions;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Validator;

namespace DirectoryService.Domain.Models;
public class Location
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string Timezone { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Location(Guid id, string name, string address, string timezone, bool isActive, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Result<Location, List<Error>> Create(string name, string address, string timezone)
    {
        var validator = new ModelValidator();

        validator.Validate(name)
            .MinLength(3)
            .MaxLength(120);

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
            createdAt: now,
            updatedAt: now
        );
    }
}
