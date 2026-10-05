using ListingSearch.Api.Data;
using ListingSearch.Api.Models;

namespace ListingSearch.Api.Services;

public sealed class ListingSearchService : IListingSearchService
{
    private readonly IListingRepository _repository;
    private readonly IClock _clock;

    public ListingSearchService(
        IListingRepository repository,
        IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public PagedResult<ListingSearchItem> Search(
        ListingSearchRequest request)
    {
        ValidateRequest(request);

        var city = request.City?.Trim();
        var keyword = request.Keyword?.Trim();

        var query = _repository.GetAll().AsEnumerable();

        // 1. Apply filters
        if (request.MinPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price <= request.MaxPrice.Value);
        }

        if (request.MinBedrooms.HasValue)
        {
            query = query.Where(x =>
                x.Bedrooms >= request.MinBedrooms.Value);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(x =>
                string.Equals(
                    x.City,
                    city,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Description.Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase));
        }

        var now = _clock.UtcNow;

        // 2. Score and sort
        var scoredResults = query
            .Select(listing => new
            {
                Listing = listing,
                Score = CalculateRelevanceScore(
                    listing,
                    request.TargetBudget,
                    now)
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Listing.ListedDate)
            .ThenBy(x => x.Listing.Source)
            .ThenBy(x => x.Listing.Id)
            .ToList();

        var totalCount = scoredResults.Count;

        // 3. Pagination
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(
                totalCount / (double)request.PageSize);

        var items = scoredResults
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ListingSearchItem
            {
                Id = x.Listing.Id,
                Source = x.Listing.Source,
                Address = x.Listing.Address,
                City = x.Listing.City,
                State = x.Listing.State,
                Zip = x.Listing.Zip,
                Price = x.Listing.Price,
                Bedrooms = x.Listing.Bedrooms,
                RelevanceScore = Math.Round(
                    x.Score,
                    2)
            })
            .ToList();

        return new PagedResult<ListingSearchItem>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    private static double CalculateRelevanceScore(
        Listing listing,
        decimal? targetBudget,
        DateTimeOffset now)
    {
        // Budget score: closer to target budget = higher score.
        double budgetScore = 0;

        if (targetBudget.HasValue && targetBudget.Value > 0)
        {
            var difference = Math.Abs(
                (double)(listing.Price - targetBudget.Value));

            budgetScore = Math.Max(
                0,
                1 - difference / (double)targetBudget.Value);
        }

        // Recency score: listings within 90 days get
        // progressively higher scores.
        var ageInDays = Math.Max(
            0,
            (now - listing.ListedDate).TotalDays);

        var recencyScore = Math.Max(
            0,
            1 - ageInDays / 90.0);

        // Budget is weighted more heavily than recency.
        return (budgetScore * 70) + (recencyScore * 30);
    }

    private static void ValidateRequest(
        ListingSearchRequest request)
    {
        if (request.Page < 1)
        {
            throw new SearchValidationException(
                "Page must be greater than or equal to 1.");
        }

        if (request.PageSize <= 0)
        {
            throw new SearchValidationException(
                "PageSize must be greater than 0.");
        }

        if (request.PageSize > 100)
        {
            throw new SearchValidationException(
                "PageSize cannot be greater than 100.");
        }

        if (request.MinPrice < 0)
        {
            throw new SearchValidationException(
                "MinPrice cannot be negative.");
        }

        if (request.MaxPrice < 0)
        {
            throw new SearchValidationException(
                "MaxPrice cannot be negative.");
        }

        if (request.MinPrice.HasValue &&
            request.MaxPrice.HasValue &&
            request.MinPrice > request.MaxPrice)
        {
            throw new SearchValidationException(
                "MinPrice cannot be greater than MaxPrice.");
        }

        if (request.MinBedrooms < 0)
        {
            throw new SearchValidationException(
                "MinBedrooms cannot be negative.");
        }

        if (request.TargetBudget.HasValue &&
            request.TargetBudget <= 0)
        {
            throw new SearchValidationException(
                "TargetBudget must be greater than 0.");
        }
    }
}