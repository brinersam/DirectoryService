namespace DirectoryService.Shared;
public record PaginatedData<T>
{
    public List<T> Data { get; init; } = null!;

    public int Page { get; init; }

    public int Take { get; init; }

    public long TotalElements { get; init; }

    public int TotalPages => TotalElements <= 0 ? 0
                                                : (int)Math.Ceiling((float)TotalElements / Take);

    public bool HasNextPage => Page < TotalPages;
}

public static class PaginationExtentions
{
    public static PaginatedData<T> ToPaginatedList<T>(this IEnumerable<T> collection, int CurrentPage, long? TotalElements)
    {
        var list = collection.ToList();

        return new PaginatedData<T>
        {
            Data = list,
            Page = Math.Max(CurrentPage, 1),
            Take = list.Count,
            TotalElements = TotalElements ?? 0,
        };
    }
}
