using MediatR;
using Meetmind.Application.Command.Audio;
using Meetmind.Application.Command.Recording;
using Meetmind.Application.QueryHandles.Recording;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("v1/meetings/{id}/[controller]")]
    [ApiController]
    public class RecordingController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly ILogger<RecordingController> _logger;

        public RecordingController(ISender mediator, ILogger<RecordingController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start(Guid id)
        {
            try
            {
                _logger.LogInformation("Starting recording for meeting with id {Id}", id);
                await _mediator.Send(new StartRecordingCommand(id));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Meeting with id {Id} not found", id);
                return NotFound("Meeting not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while starting recording for meeting with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished Start");
            }

        }

        [HttpPost("pause")]
        public async Task<IActionResult> Pause(Guid id)
        {
            try
            {
                _logger.LogInformation("Pausing recording for meeting with id {Id}", id);
                await _mediator.Send(new PauseRecordingCommand(id));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Meeting with id {Id} not found", id);
                return NotFound("Meeting not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while pausing recording for meeting with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished Pause");
            }
           
        }

        [HttpPost("resume")]
        public async Task<IActionResult> Resume(Guid id)
        {
            try
            {
                _logger.LogInformation("Resuming recording for meeting with id {Id}", id);

                await _mediator.Send(new ResumeRecordingCommand(id));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Meeting with id {Id} not found", id);
                return NotFound("Meeting not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while pausing recording for meeting with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished Pause");
            }
           
        }

        [HttpPost("stop")]
        public async Task<IActionResult> Stop(Guid id)
        {
            try
            {
                _logger.LogInformation("Stopping recording for meeting with id {Id}", id);

                await _mediator.Send(new StopRecordingCommand(id, DateTime.UtcNow));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Meeting with id {Id} not found", id);
                return NotFound("Meeting not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while pausing recording for meeting with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished Pause");
            }
           
        }

        [HttpGet("status")]
        public async Task<ActionResult<string>> Status(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting recording status for meeting with id {Id}", id);

                var status = await _mediator.Send(new GetRecordingStatusQuery(id));
                return Ok(status);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Meeting with id {Id} not found", id);
                return NotFound("Meeting not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while pausing recording for meeting with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished Pause");
            }
        }

        [HttpPost("fragment")]
        public async Task<IActionResult> UploadFragment(Guid id,int sequenceNumber, IFormFile audioChunk)
        {
            await _mediator.Send(new AddAudioFragmentCommand(id, sequenceNumber, audioChunk));

            return Ok();
        }
    }
}
