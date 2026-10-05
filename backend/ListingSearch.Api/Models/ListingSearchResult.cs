namespace ListingSearch.Api.Models;

public sealed class ListingSearchItem
{
    public string Id { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Zip { get; init; } = string.Empty;

    public decimal Price { get; init; }
    public int Bedrooms { get; init; }

    public double RelevanceScore { get; init; }
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}