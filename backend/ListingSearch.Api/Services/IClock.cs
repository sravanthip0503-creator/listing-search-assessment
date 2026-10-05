namespace ListingSearch.Api.Services;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}