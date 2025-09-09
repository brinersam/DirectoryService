namespace DirectoryService.Contracts.Dtos;
public record LocationDto(
    Guid Id,
    LocationNameDto Name,
    string Address,
    string Timezone,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
