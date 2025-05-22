using MediatR;
using Meetmind.Application.Command.Audio;
using Meetmind.Application.QueryHandles.Mettings;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class AudioController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly ILogger<AudioController> _logger;

        public AudioController(ISender mediator, ILogger<AudioController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("{meetingId}")]
        public async Task<IActionResult> GetAudioByID([FromRoute] Guid meetingId)
        {
            // Recherche du meeting et récupération du chemin du fichier audio
            var meeting = await _mediator.Send(new GetMeetingByIdQuery(meetingId));

            if (string.IsNullOrWhiteSpace(meeting.AudioPath))
            {
                _logger.LogDebug($"Not Audio with id {meetingId} found");
                return NotFound($"Not Audio with id {meetingId} found");
            }

            if (!System.IO.File.Exists(meeting.AudioPath))
            {
                _logger.LogDebug($"Audio file not found: {meeting.AudioPath}");
                return NotFound($"Audio non trouvé pour la réunion {meetingId}");
            }

            var fileName = Path.GetFileName(meeting.AudioPath);

            // Transmission du fichier au client (stream ou download)
            return PhysicalFile(meeting.AudioPath, "audio/wav", fileName, enableRangeProcessing: true);
        }

        /// <summary>
        /// Supprime le fichier audio associé à une réunion
        /// </summary>
        [HttpDelete("{meetingId:guid}")]
        public async Task<IActionResult> DeleteAudio([FromRoute] Guid meetingId, string tilte)
        {
            try
            {
               await  _mediator.Send(new DeleteAudioCommand(meetingId));
                return NoContent();
            }
            catch (KeyNotFoundException ke)
            {
                _logger.LogError(ke, "Audio not found");
                return NotFound(ke.Message);
            }
            catch (FileNotFoundException fe)
            {
                _logger.LogError(fe, "Audio not found");
                return NotFound(fe.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while deleting the audio");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

        }
    }
}
