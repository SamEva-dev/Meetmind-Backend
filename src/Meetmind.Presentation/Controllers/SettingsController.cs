using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetSettings()
        {
            return Ok(new
            {
                Language = "fr",
                AutoTranscription = true
            });
        }
    }
}
