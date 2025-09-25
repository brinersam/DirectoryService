namespace DirectoryService.Contracts.Requests;
public record CreateLocationRequest(
    string LocationName,
    string Address,
    string Timezone);