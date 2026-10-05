using ListingSearch.Api.Data;
using ListingSearch.Api.Models;
using ListingSearch.Api.Services;

namespace ListingSearch.Tests;

public class ListingSearchServiceTests
{
    private readonly FakeClock _clock;
    private readonly ListingSearchService _service;

    public ListingSearchServiceTests()
    {
        _clock = new FakeClock(
            new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero));

        var repository = new TestListingRepository();

        _service = new ListingSearchService(
            repository,
            _clock);
    }

    [Fact]
    public void Search_FiltersByCity_CaseInsensitive()
    {
        var request = new ListingSearchRequest
        {
            City = "dallas",
            Page = 1,
            PageSize = 10
        };

        var result = _service.Search(request);

        Assert.Equal(2, result.TotalCount);
        Assert.All(
            result.Items,
            item => Assert.Equal(
                "Dallas",
                item.City));
    }

    [Fact]
    public void Search_ReturnsEmpty_WhenNoListingsMatch()
    {
        var request = new ListingSearchRequest
        {
            City = "Houston",
            Page = 1,
            PageSize = 10
        };

        var result = _service.Search(request);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void Search_Throws_WhenMinPriceGreaterThanMaxPrice()
    {
        var request = new ListingSearchRequest
        {
            MinPrice = 600000,
            MaxPrice = 400000,
            Page = 1,
            PageSize = 10
        };

        var exception = Assert.Throws<SearchValidationException>(
            () => _service.Search(request));

        Assert.Equal(
            "MinPrice cannot be greater than MaxPrice.",
            exception.Message);
    }

    [Fact]
    public void Search_Throws_WhenPageSizeIsInvalid()
    {
        var request = new ListingSearchRequest
        {
            Page = 1,
            PageSize = 0
        };

        var exception = Assert.Throws<SearchValidationException>(
            () => _service.Search(request));

        Assert.Equal(
            "PageSize must be greater than 0.",
            exception.Message);
    }

    [Fact]
    public void Search_PaginatesCorrectly()
    {
        var request = new ListingSearchRequest
        {
            Page = 2,
            PageSize = 5
        };

        var result = _service.Search(request);

        Assert.Equal(12, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(2, result.Page);
    }

    [Fact]
    public void Search_ReturnsRemainingItems_OnLastPage()
    {
        var request = new ListingSearchRequest
        {
            Page = 3,
            PageSize = 5
        };

        var result = _service.Search(request);

        Assert.Equal(12, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public void Search_ReturnsEmpty_WhenPageIsBeyondLastPage()
    {
        var request = new ListingSearchRequest
        {
            Page = 4,
            PageSize = 5
        };

        var result = _service.Search(request);

        Assert.Empty(result.Items);
        Assert.Equal(12, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void Search_FiltersByKeyword_CaseInsensitive()
    {
        var request = new ListingSearchRequest
        {
            Keyword = "KITCHEN",
            Page = 1,
            PageSize = 20
        };

        var result = _service.Search(request);

        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public void Search_FiltersByMinimumBedrooms()
    {
        var request = new ListingSearchRequest
        {
            MinBedrooms = 4,
            Page = 1,
            PageSize = 20
        };

        var result = _service.Search(request);

        Assert.NotEmpty(result.Items);
        Assert.All(
            result.Items,
            item => Assert.True(item.Bedrooms >= 4));
    }

    [Fact]
public void Search_RanksExactBudgetHighly()
{
    var service = CreateService();

    var request = new ListingSearchRequest
    {
        TargetBudget = 450000,
        Page = 1,
        PageSize = 5
    };

    var result = service.Search(request);

    Assert.NotEmpty(result.Items);

    var firstItem = result.Items.First();

    Assert.Equal("1012", firstItem.Id);
    Assert.Equal(450000, firstItem.Price);
}
    [Fact]
    public void Search_ProducesDeterministicOrdering_ForTiedScores()
    {
        var request = new ListingSearchRequest
        {
            TargetBudget = 515000,
            Page = 1,
            PageSize = 20
        };

        var firstResult = _service.Search(request);
        var secondResult = _service.Search(request);

        Assert.Equal(
            firstResult.Items.Select(x => x.Id),
            secondResult.Items.Select(x => x.Id));
    }

    private sealed class TestListingRepository : IListingRepository
    {
        private readonly IReadOnlyList<Listing> _listings =
        [
            new Listing
            {
                Id = "1001",
                Source = "MLS_A",
                Address = "120 Main Street",
                City = "Dallas",
                State = "TX",
                Zip = "75201",
                Price = 425000,
                Bedrooms = 3,
                Bathrooms = 2.5m,
                Sqft = 1850,
                ListedDate = new DateTimeOffset(
                    2026, 9, 20, 0, 0, 0,
                    TimeSpan.Zero),
                Status = "active",
                Description =
                    "Beautiful Dallas home with updated kitchen."
            },

            new Listing
            {
                Id = "1002",
                Source = "MLS_A",
                Address = "450 Oak Avenue",
                City = "Plano",
                State = "TX",
                Zip = "75024",
                Price = 515000,
                Bedrooms = 4,
                Bathrooms = 3,
                Sqft = 2450,
                ListedDate = new DateTimeOffset(
                    2026, 9, 15, 0, 0, 0,
                    TimeSpan.Zero),
                Status = "active",
                Description =
                    "Spacious Plano family home with renovated kitchen."
            },

            new Listing
            {
                Id = "1003",
                Source = "MLS_B",
                Address = "88 Lake View Drive",
                City = "Frisco",
                State = "TX",
                Zip = "75034",
                Price = 625000,
                Bedrooms = 4,
                Bathrooms = 3.5m,
                Sqft = 2900,
                ListedDate = new DateTimeOffset(
                    2026, 9, 25, 0, 0, 0,
                    TimeSpan.Zero),
                Status = "active",
                Description =
                    "Modern Frisco property with smart home technology."
            },

            new Listing
            {
                Id = "1004",
                Source = "MLS_A",
                Address = "230 Elm Street",
                City = "Austin",
                State = "TX",
                Zip = "78701",
                Price = 475000,
                Bedrooms = 3,
                Bathrooms = 2,
                Sqft = 1725,
                ListedDate = new DateTimeOffset(
                    2026, 9, 10, 0, 0, 0,
                    TimeSpan.Zero),
                Status = "active",
                Description =
                    "Charming Austin home with renovated kitchen."
            },

            new Listing
            {
                Id = "1005",
                Source = "MLS_B",
                Address = "710 Cedar Lane",
                City = "McKinney",
                State = "TX",
                Zip = "75070",
                Price = 390000,
                Bedrooms = 3,
                Bathrooms = 2,
                Sqft = 1950,
                ListedDate = new DateTimeOffset(
                    2026, 8, 28, 0, 0, 0,
                    TimeSpan.Zero),
                Status = "active",
                Description =
                    "Well maintained McKinney home with updated flooring."
            },

            new Listing
            {
                Id = "1006",
                Source = "MLS_A",
                Address = "55 River Road",
                City = "Fort Worth",
                State = "TX",
                Zip = "76102",
                Price = 350000,
                Bedrooms = 2,
                Bathrooms = 2,
                Sqft = 1450,
                ListedDate = new DateTimeOffset(
                    2026, 9, 5, 0, 0, 0,
                    TimeSpan.Zero),
                Status = "pending",
                Description =
                    "Cozy Fort Worth property close to restaurants."
            },

            new Listing
            {
                Id = "1007",
                Source = "MLS_B",
                Address = "910 Park Boulevard",
                City = "Richardson",
                State = "TX",
                Zip = "75080",
                Price = 410000,
                Bedrooms = 3,
                Bathrooms = 2,
                Sqft = 1780,
                ListedDate = new DateTimeOffset(
                    2026, 9, 18, 0, 0, 0,
                    TimeSpan.Zero),
                Status = "active",
                Description =
                    "Updated Richardson home with modern kitchen."
            },

            new Listing
            {
                Id = "1008",
                Source = "MLS_A",
                Address = "340 Sunset Boulevard",
                City = "Irving",
                State = "TX",
                Zip = "75039",
                Price = 550000,
                Bedrooms = 4,
                Bathrooms = 3,
                Sqft = 2600,
                ListedDate = new DateTimeOffset(
                    2026, 9, 12, 0, 0, 0,
                    TimeSpan.Zero),
                Status = "active",
                Description =
                    "Contemporary Irving home with luxury finishes."
            },

            new Listing
            {
                Id = "1009",
                Source = "MLS_B",
                Address = "75 Highland Court",
                City = "Denton",
                State = "TX",
                Zip = "76201",
                Price = 325000,
                Bedrooms = 3,
                Bathrooms = 2,
                Sqft = 1650,
                ListedDate = new DateTimeOffset(
                    2026, 8, 20, 0, 0, 0,
                    TimeSpan.Zero),
                Status = "active",
                Description =
                    "Affordable Denton home with updated kitchen."
            },

            new Listing
            {
                Id = "1010",
                Source = "MLS_A",
                Address = "500 Preston Road",
                City = "Plano",
                State = "TX",
                Zip = "75093",
                Price = 515000,
                Bedrooms = 4,
                Bathrooms = 3,
                Sqft = 2500,
                ListedDate = new DateTimeOffset(
                    2026, 9, 15, 0, 0, 0,
                    TimeSpan.Zero),
                Status = "active",
                Description =
                    "Beautiful Plano home with renovated kitchen."
            },

            new Listing
            {
                Id = "1011",
                Source = "MLS_B",
                Address = "640 Market Street",
                City = "Dallas",
                State = "TX",
                Zip = "75202",
                Price = 465000,
                Bedrooms = 3,
                Bathrooms = 2,
                Sqft = 1800,
                ListedDate = new DateTimeOffset(
                    2026, 9, 22, 0, 0, 0,
                    TimeSpan.Zero),
                Status = "sold",
                Description =
                    "Modern Dallas property with open floor plan."
            },

            new Listing
            {
                Id = "1012",
                Source = "MLS_A",
                Address = "82 Meadow Lane",
                City = "Celina",
                State = "TX",
                Zip = "75009",
                Price = 450000,
                Bedrooms = 3,
                Bathrooms = 2.5m,
                Sqft = 2100,
                ListedDate = new DateTimeOffset(
                    2026, 9, 27, 0, 0, 0,
                    TimeSpan.Zero),
                Status = "active",
                Description =
                    "Newer Celina home with modern kitchen."
            }
        ];

        public IReadOnlyList<Listing> GetAll()
        {
            return _listings;
        }
    }

    private sealed class FakeClock : IClock
    {
        public FakeClock(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; }
    }

    private static ListingSearchService CreateService()
{
    return new ListingSearchService(
        new TestListingRepository(),
        new FakeClock(
            new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero)));
}
}