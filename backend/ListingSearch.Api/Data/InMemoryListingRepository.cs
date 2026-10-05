using System.Text.Json;
using ListingSearch.Api.Models;

namespace ListingSearch.Api.Data;

public sealed class InMemoryListingRepository : IListingRepository
{
    private readonly IReadOnlyList<Listing> _listings;

    public InMemoryListingRepository(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Listing data file was not found: {filePath}");
        }

        var json = File.ReadAllText(filePath);

        _listings = JsonSerializer.Deserialize<List<Listing>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? [];
    }

    public IReadOnlyList<Listing> GetAll()
    {
        return _listings;
    }
}