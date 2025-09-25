using CSharpFunctionalExtensions;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Validator;
using System.Text.Json.Serialization;

namespace DirectoryService.Domain.Models.Departments.ValueObject;
public class DepartmentPath
{
    public string Value { get; init; }

    [JsonConstructor]
    private DepartmentPath(string value)
    {
        Value = value;
    }

    public static Result<DepartmentPath, List<Error>> Create(string path)
    {
        var validator = new ModelValidator();
        validator.Validate(path)
            .ContainsNone(' ');

        var validationResult = validator.ValidateAll(out bool isError);

        if (isError)
            return validationResult.SelectMany(x => x).ToList();

        return new DepartmentPath(path);
    }
}
