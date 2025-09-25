using CSharpFunctionalExtensions;
using DirectoryService.Application.Interfaces;
using DirectoryService.Shared.ErrorClasses;
using FluentValidation;

namespace DirectoryService.Application.Features.Commands.MoveDepartmentToParent;

public class SetDepartmentParentHandler
{
    private readonly IDepartmentRepository _departmentRepository;

    public SetDepartmentParentHandler(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<UnitResult<IEnumerable<Error>>> HandleAsync(
        SetDepartmentParentCommand cmd,
        CancellationToken ct = default)
    {
        var childDepartment = await _departmentRepository.GetDepartmentAsync(cmd.departmentId, true, ct);
        if (childDepartment is null)
            return Error.NotFound($"Requested department for move is not found").ToSingleErrorArray();

        var moveRes = await _departmentRepository.MoveDepartmentToParent(
            cmd.departmentId,
            cmd.request.parentDepartmentId,
            ct);
        if (moveRes.IsFailure)
            return moveRes.Error.ToSingleErrorArray();
        return Result.Success<IEnumerable<Error>>();
    }
}