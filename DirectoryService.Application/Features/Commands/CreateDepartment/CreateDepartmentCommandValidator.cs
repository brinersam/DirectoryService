using DirectoryService.Shared.Validator;
using FluentValidation;

namespace DirectoryService.Application.Features.Commands.CreateDepartment;
public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.request.Name)
            .MinimumLength(3)
            .MaximumLength(150);

        RuleFor(x => x.request.Identifier)
            .MinimumLength(3)
            .MaximumLength(150)
            .Must(x =>
            {
                var validator = new ModelValidator();
                validator.Validate(x)
                    .NotNullOrEmpty()
                    .MinLength(3)
                    .MaxLength(150)
                    .ContainsNone(' ')
                    .HasFormat(FormatRulesEnum.Latin);
                var errs = validator.ValidateAll(out bool isError);
                return isError == false;
            });

        RuleFor(x => x.request.LocationsId)
            .NotEmpty()
            .Must(x => x.Distinct().Count() == x.Count());
    }
}
