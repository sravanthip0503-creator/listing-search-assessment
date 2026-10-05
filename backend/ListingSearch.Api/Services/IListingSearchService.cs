using ListingSearch.Api.Models;

namespace ListingSearch.Api.Services;

public interface IListingSearchService
{
    PagedResult<ListingSearchItem> Search(ListingSearchRequest request);
}