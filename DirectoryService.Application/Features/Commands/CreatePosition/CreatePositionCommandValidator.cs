using CSharpFunctionalExtensions;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models;
using DirectoryService.Shared.ErrorClasses;
using FluentValidation;

namespace DirectoryService.Application.Features.Commands.CreatePosition;
public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator(
        IPositionRepository _positionRepository,
        IDepartmentRepository _departmentRepository)
    {
        RuleFor(x => x.request.Name)
            .MinimumLength(3)
            .MaximumLength(100)
            .MustAsync(async (x,ct) =>
            {
                Result<Position, Error> existingPositonRes = await _positionRepository.GetPositionAsync(name: x, ct: ct);
                return existingPositonRes.IsFailure;
            });

        RuleFor(x => x.request.DepartmentIds)
            .NotEmpty()
            .Must(x => x.Distinct().Count() == x.Count())
            .MustAsync(async (x,ct) => 
            {
                var departmentsThatExistAndValid = await _departmentRepository.GetDepartmentsAsync(x, true, ct);
                return departmentsThatExistAndValid.IsSuccess;
            });
    }
}
