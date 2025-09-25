namespace DirectoryService.Application.Features.Commands.CreateLocation;
public record CreateLocationCommand(
    string LocationName,
    string Address,
    string Timezone);
