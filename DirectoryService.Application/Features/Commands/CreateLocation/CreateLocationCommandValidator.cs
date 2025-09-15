using DirectoryService.Domain.Models.Locations.ValueObject;
using DirectoryService.Shared.Validator;
using FluentValidation;

namespace DirectoryService.Application.Features.Commands.CreateLocation;
public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.Address)
            .MaximumLength(120);

        RuleFor(x => x.Timezone)
            .NotEmpty()
            .Matches(@"([A-Za-z_\-]+\/[A-Za-z_\-]+(?:\/[A-Za-z_\-]+)?)|(?:Etc\/[A-Za-z0-9+\-]+(?:\/[A-Za-z0-9]+)?|(?:CET|CST6CDT|EET|EST|EST5EDT|MET|MST|MST7MDT|PST8PDT|HST))$");

        RuleFor(x => x.LocationName)
            .ValidateValueObj(LocationName.Create);
    }
}
