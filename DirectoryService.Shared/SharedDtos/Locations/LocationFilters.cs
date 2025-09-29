namespace DirectoryService.Shared.SharedDtos.Locations;
public record LocationFilters(
    Guid[]? DepartmentIds,
    string? SearchName,
    bool? IsActive)
{
    public IEnumerable<(string paramName, object? value)> IterateOnNonNullValues()
    {
        if (DepartmentIds is not null)
            yield return (nameof(DepartmentIds), DepartmentIds);

        if (SearchName is not null)
            yield return (nameof(SearchName), SearchName);

        if (IsActive.HasValue)
            yield return (nameof(IsActive), IsActive);
    }
}
