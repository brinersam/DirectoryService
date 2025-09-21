using DirectoryService.Application.Interfaces;
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
            .MaximumLength(100);

        RuleFor(x => x.request.DepartmentIds)
            .NotEmpty()
            .Must(x => x.Distinct().Count() == x.Count());
    }
}
