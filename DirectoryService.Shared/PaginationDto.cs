namespace DirectoryService.Shared;
public record PaginationDto(
    int Page = AppConsts.Page,
    int Take = AppConsts.Take);
