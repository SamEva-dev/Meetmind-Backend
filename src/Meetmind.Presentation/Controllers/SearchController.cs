using Meetmind.Application.Common.Interfaces;
using Meetmind.Application.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("v1/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _search;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService search, ILogger<SearchController> logger)
    {
        _search = search;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<SearchEntry>>> Search(
        [FromQuery] string q,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest("Query parameter `q` is required.");
        }

        _logger.LogInformation("Search requested: '{Query}'", q);

        var results = await _search.SearchAsync(q, from, to);
        return Ok(results);
    }
}