using ListingSearch.Api.Models;
using ListingSearch.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ListingSearch.Api.Controllers;

[ApiController]
[Route("api/listings")]
public sealed class ListingsController : ControllerBase
{
    private readonly IListingSearchService _searchService;

    public ListingsController(
        IListingSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("search")]
    public ActionResult<PagedResult<ListingSearchItem>> Search(
        [FromQuery] ListingSearchRequest request)
    {
        try
        {
            var result = _searchService.Search(request);

            return Ok(result);
        }
        catch (SearchValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid search request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}