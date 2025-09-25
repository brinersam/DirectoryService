using CSharpFunctionalExtensions;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Framework.DbConnection;
using DirectoryService.Shared.Validator;
using FluentValidation;

namespace DirectoryService.Application.Features.Commands.CreatePosition;

public class CreatePositionHandler
{
    private readonly IValidator<CreatePositionCommand> _cmdValidator;
    private readonly IDepartmentRepository _repositoryDepartment;
    private readonly AppDb _appDb;
    private readonly IPositionRepository _repositoryPosition;

    public CreatePositionHandler(
        IValidator<CreatePositionCommand> cmdValidator,
        IDepartmentRepository repositoryDepartment,
        AppDb appDb,
        IPositionRepository repositoryPosition)
    {
        _cmdValidator = cmdValidator;
        _repositoryDepartment = repositoryDepartment;
        _appDb = appDb;
        _repositoryPosition = repositoryPosition;
    }

    public async Task<Result<Guid, IEnumerable<Error>>> HandleAsync(
        CreatePositionCommand cmd,
        CancellationToken ct = default)
    {
        var validationResults = await _cmdValidator.ValidateAsync(cmd, ct);
        if (validationResults.IsValid == false)
            return validationResults.Errors.ToAppErrors().ToList();

        var existingPositonRes = await _repositoryPosition.GetPositionAsync(name: cmd.request.Name, ct: ct);
        if (existingPositonRes.IsSuccess)
            return new[] {Error.Validation($"Имя позиции [{cmd.request.Name}] уже используется!")};

        var validDepartments = await _repositoryDepartment.GetDepartmentsAsync(cmd.request.DepartmentIds, true, ct);
        if (validDepartments.IsFailure)
            return new[] { Error.Validation($"Введенные департаменты не валидны!") };

        var validDepartmentIds = validDepartments.Value.Select(x => x.Id).ToHashSet();
        var invalidDepartments = cmd.request.DepartmentIds
            .Where(requestedDepartmentId => validDepartmentIds.Contains(requestedDepartmentId) == false)
            .ToList();

        if (invalidDepartments.Any())
            return new[] { Error.Validation($"Введенные департаменты с id : [{String.Join(';', invalidDepartments)}] не валидны!") };

        var positionRes = Position.Create(
            cmd.request.Name,
            cmd.request.DepartmentIds,
            cmd.request.Description);
        if (positionRes.IsFailure)
            return positionRes.Error;

        var addPositionResult = await _repositoryPosition.AddPositionAsync(positionRes.Value, ct);
        if (addPositionResult.IsFailure)
            return new[] { addPositionResult.Error };

        return positionRes.Value.Id;
    }
}