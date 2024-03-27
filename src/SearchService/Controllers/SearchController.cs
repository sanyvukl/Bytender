using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Helpers;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItemsAsync([FromQuery] SearchParams searchParams)
    {
        // Add secutiry checks here
        bool hasAccess = true;

        if (!hasAccess)
        {
            return Unauthorized();
        }

        try
        {
            var query = this.GetFilteredQuery(searchParams, true);
            var totalCountQuery = this.GetFilteredQuery(searchParams, false);

            var queryResult = await query.ExecuteAsync();
            if (queryResult == null)
            {
                return NotFound();
            }

            int totalCount = (await totalCountQuery.ExecuteAsync()).Count;
            return Ok(new
            {
                results = queryResult,
                pageCount = queryResult.Count(),
                totalCount = totalCount
            });
        }
        catch (Exception ex)
        {
            ExceptionHandler.HandleException(ex, nameof(SearchItemsAsync));
            return StatusCode(500, "Internal server error");
        }
    }


    private Find<Item> GetFilteredQuery(SearchParams searchParams, bool includeSkipTake = false)
    {
        var query = DB.Find<Item>();
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm);
            query.SortByTextScore();
        }

        switch (searchParams.OrderBy)
        {
            case "make":
                query.Sort(x => x.Ascending(a => a.Make));
                break;
            case "new":
                query.Sort(x => x.Descending(a => a.Year));
                break;
            default:
                query.Sort(x => x.Ascending(a => a.Make));
                break;
        }

        switch (searchParams.FilterBy)
        {
            case "finished":
                query.Match(x => x.AuctionEnd < DateTime.UtcNow);
                break;
            case "endingSoon":
                query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddDays(1) && x.AuctionEnd > DateTime.UtcNow);
                break;
            default:
                query.Match(x => x.AuctionEnd > DateTime.UtcNow);
                break;
        }

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => string.Equals(x.Seller, searchParams.Seller, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => string.Equals(x.Winner, searchParams.Winner, StringComparison.OrdinalIgnoreCase));
        }

        if (includeSkipTake)
        {
            query.Skip(searchParams.PageSize * (searchParams.PageNumber - 1));
            query.Limit(searchParams.PageSize);
        }

        return query;
    }
}
