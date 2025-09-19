using CSharpFunctionalExtensions;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models.Departments;
using DirectoryService.Domain.Models.Departments.ValueObject;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Validator;
using FluentValidation;

namespace DirectoryService.Application.Features.Commands.CreateDepartment;

public class CreateDepartmentHandler
{
    private readonly IValidator<CreateDepartmentCommand> _cmdValidator;
    private readonly IDepartmentRepository _repository;

    public CreateDepartmentHandler(
        IValidator<CreateDepartmentCommand> cmdValidator,
        IDepartmentRepository repository)
    {
        _cmdValidator = cmdValidator;
        _repository = repository;
    }

    public async Task<Result<Guid, IEnumerable<Error>>> HandleAsync(
        CreateDepartmentCommand cmd,
        CancellationToken ct = default)
    {
        var validationResults = await _cmdValidator.ValidateAsync(cmd, ct);
        if (validationResults.IsValid == false)
            return validationResults.Errors.ToAppErrors().ToList();


        short depth = 0;
        Guid? parentId = null;
        var path = cmd.request.Identifier;
        if (cmd.request.ParentId is not null)
        {
            var getParentDepartment = await _repository.GetDepartmentAsync((Guid)cmd.request.ParentId);
            if (getParentDepartment.IsSuccess)
            {
                var parentDepartment = getParentDepartment.Value;
                depth = (short)(parentDepartment.Depth + 1);
                path = $"{parentDepartment.Path.Value}.{cmd.request.Identifier}";
            }
        }

        var createPath = DepartmentPath.Create(path);
        if (createPath.IsFailure)
            return createPath.Error;

        var departmentRes = Department.Create(
            cmd.request.Name,
            cmd.request.Identifier,
            createPath.Value,
            depth,
            parentId);
        if (departmentRes.IsFailure)
            return departmentRes.Error;

        var addDeptRes = await _repository.AddDepartmentAsync(departmentRes.Value);
        if (addDeptRes.IsFailure)
            return new[] { addDeptRes.Error };

        return departmentRes.Value.Id;
    }
}