namespace DirectoryService.Infrastructure.Database.Datamodels;
public record PositionDataModel(
        Guid Id,
        string Name,
        string? Description,
        bool IsActive,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);
