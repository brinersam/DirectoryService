using FluentValidation;

namespace DirectoryService.Application.Features.Commands.UpdateDepartment;
public class UpdateDepartmentLocationsCommandValidation : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentLocationsCommandValidation()
    {
        RuleFor(x => x.DepartmentId)
            .NotEqual(default(Guid));

        RuleFor(x => x.Request.LocationIds)
            .NotEmpty()
            .Must(x => x.Distinct().Count() == x.Length);
    }
}
