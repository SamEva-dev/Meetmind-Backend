using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var results = await _search.SearchAsync(q, from, to);
            return Ok(results);
        }
    }
}
