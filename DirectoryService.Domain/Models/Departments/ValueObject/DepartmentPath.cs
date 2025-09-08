using CSharpFunctionalExtensions;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.ModelInterfaces;
using DirectoryService.Shared.Validator;

namespace DirectoryService.Domain.Models.Departments.ValueObject;
public class DepartmentPath : IJsonbObject
{
    public string Value { get; init; }

    private DepartmentPath(string path)
    {
        Value = path;
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
