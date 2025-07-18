using MediatR;
using Meetmind.Application.Command.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly ILogger<AudioController> _logger;

        public AdminController(ISender mediator, ILogger<AudioController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpDelete("drop-meetings")]
        public async Task<IActionResult> DropMeetingsTable()
        {
            try
            {
                await _mediator.Send(new DropMeetingCommand());
                return Ok("Table 'Meetings' supprimée avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la suppression de la table : {ex.Message}");
            }
        }

        [HttpDelete("clear-meetings")]
        public async Task<IActionResult> ClearMeetingsTable()
        {
            try
            {
                await _mediator.Send(new DeleteMeetingCommand());
                return Ok("Toutes les données de la table 'Meetings' ont été supprimées.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur : {ex.Message}");
            }
        }
    }
}
