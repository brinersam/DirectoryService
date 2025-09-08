using CSharpFunctionalExtensions;
using DirectoryService.Domain.Models.Departments.ValueObject;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Validator;

namespace DirectoryService.Domain.Models.Departments;
public class Department
{
    public Guid Id { get; init; }

    public string Name { get; private set; }

    public string Identifier { get; private set; }

    public Guid? ParentId { get; private set; }

    public DepartmentPath Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    private Department(
        Guid id,
        string name,
        string identifier,
        Guid? parentId,
        DepartmentPath path,
        short depth)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Result<Department, List<Error>> Create(
        string name,
        string identifier,
        DepartmentPath path,
        short depth,
        Guid? parentId = null)
    {
        var validator = new ModelValidator();

        validator.Validate(name)
            .NotNullOrEmpty()
            .MinLength(3)
            .MaxLength(150);

        validator.Validate(identifier)
            .NotNullOrEmpty()
            .MinLength(3)
            .MaxLength(150)
            .HasFormat(FormatRulesEnum.Latin);

        var validationResult = validator.ValidateAll(out bool isError);

        if (isError)
            return validationResult.SelectMany(x => x).ToList();

        return new Department(
            Guid.NewGuid(),
            name,
            identifier,
            parentId,
            path,
            depth);
    }
}
