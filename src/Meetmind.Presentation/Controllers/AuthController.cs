using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpGet("whoami")]
        [Authorize]
        public IActionResult WhoAmI()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(new
            {
                Authenticated = User.Identity?.IsAuthenticated,
                Name = User.Identity?.Name,
                Claims = claims
            });
        }

        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping() => Ok("pong");
    }
}
