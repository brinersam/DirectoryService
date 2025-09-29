namespace DirectoryService.Contracts.Dtos;
public record LocationDto
{
    public Guid Id { get; set; }

    public string LocationName { get; set; }

    public string Address { get; set; }

    public string Timezone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
