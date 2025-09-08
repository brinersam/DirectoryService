using CSharpFunctionalExtensions;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Validator;

namespace DirectoryService.Domain.Models;
public class Position
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    private Position(Guid id, string name, string? description, bool isActive, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAtUtc = createdAt;
        UpdatedAtUtc = updatedAt;
    }

    public static Result<Position, List<Error>> Create(string name, string? description = null)
    {
        var validator = new ModelValidator();

        validator.Validate(name)
            .MinLength(3)
            .MaxLength(100);

        validator.Validate(description)
            .MaxLength(1000);

        var errors = validator.ValidateAll(out bool isError);
        if (isError)
            return errors.SelectMany(x => x).ToList();

        var now = DateTime.UtcNow;

        return new Position(
            Guid.NewGuid(),
            name,
            description,
            isActive: true,
            createdAt: now,
            updatedAt: now
        );
    }
}
