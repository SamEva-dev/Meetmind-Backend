using MediatR;
using Meetmind.Application.Commands;
using Meetmind.Application.Queries;
using Meetmind.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("v1/meetings/{id}/transcript")]
    [ApiController]
    public class TranscriptionController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly ILogger<TranscriptionController> _logger;

        public TranscriptionController(ISender sender, ILogger<TranscriptionController> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTranscript(Guid id)
        {
            try
            {
                var content = await _sender.Send(new GetTranscriptQuery(id));
                return Ok(content);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Meeting not found.");
            }
            catch (FileNotFoundException)
            {
                return StatusCode(500, "Transcript file missing.");
            }
            catch (InvalidOperationException)
            {
                return StatusCode(425, "Transcript not ready.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> TriggerTranscription(Guid id)
        {
            try
            {
                await _sender.Send(new TriggerTranscriptionCommand(id));
                return Accepted(); // 202
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Meeting not found.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid state for Meeting {MeetingId}", id);
                return Conflict(ex.Message);
            }
        }
    }
}
