using CSharpFunctionalExtensions;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Validator;
using System.Text.Json.Serialization;

namespace DirectoryService.Domain.Models;
public class Position
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyList<Guid> DepartmentIds => _departmentIds.AsReadOnly();

    private List<Guid> _departmentIds = [];

    [JsonConstructor]
    private Position() { }

    // dont use outside of db reads
    public Position(Guid id, string name, string? description, bool isActive, DateTime createdAt, DateTime updatedAt, IEnumerable<Guid> departmentIds)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAtUtc = createdAt;
        UpdatedAtUtc = updatedAt;
        _departmentIds = departmentIds.ToList();
    }

    public static Result<Position, List<Error>> Create(string name, IEnumerable<Guid> departmentIds, string? description = null)
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
            updatedAt: now,
            departmentIds: departmentIds
        );
    }
}
