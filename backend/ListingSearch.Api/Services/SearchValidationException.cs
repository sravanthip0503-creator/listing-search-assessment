namespace ListingSearch.Api.Services;

public sealed class SearchValidationException : Exception
{
    public SearchValidationException(string message)
        : base(message)
    {
    }
}