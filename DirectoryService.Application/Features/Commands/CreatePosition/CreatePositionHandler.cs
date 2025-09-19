using CSharpFunctionalExtensions;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Framework;
using DirectoryService.Shared.Validator;
using FluentValidation;

namespace DirectoryService.Application.Features.Commands.CreatePosition;

public class CreatePositionHandler
{
    private readonly IValidator<CreatePositionCommand> _cmdValidator;
    private readonly AppDb _appDb;
    private readonly IPositionRepository _repositoryPosition;

    public CreatePositionHandler(
        IValidator<CreatePositionCommand> cmdValidator,
        IDepartmentRepository repositoryDepartment,
        AppDb appDb,
        IPositionRepository repositoryPosition)
    {
        _cmdValidator = cmdValidator;
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