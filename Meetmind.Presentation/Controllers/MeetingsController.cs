using MediatR;
using Meetmind.Application.Command.Meetings;
using Meetmind.Application.Dto;
using Meetmind.Application.QueryHandles.Meetings;
using Meetmind.Application.QueryHandles.Mettings;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class MeetingsController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly ILogger<MeetingsController> _logger;

        public MeetingsController(ISender mediator, ILogger<MeetingsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MeetingDto>> GetById(Guid id)
        {
            try
            {
                _logger.LogInformation("Get meeting by id {Id}", id);
                var result = await _mediator.Send(new GetMeetingByIdQuery(id));
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Meeting with id {Id} not found", id);
                return NotFound("Meeting not found.");
            }
            catch (Exception)
            {
                _logger.LogError("Error occurred while getting meeting by id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished GetById");
            }

        }

        [HttpGet("today")]
        public async Task<ActionResult<List<MeetingDto>>> GetToday()
        {
            try
            {
                var result = await _mediator.Send(new GetTodayMeetingsQuery());
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("No meetings found for today");
                return NotFound("Meeting not found  for today");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting today's meetings");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished GetToday");
            }

        }
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                _logger.LogInformation("Delete meeting with id {Id}", id);
                var result = await _mediator.Send(new DeleteMeetingCommand(id));
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Meeting with id {Id} not found", id);
                return NotFound("Meeting not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting meeting with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished Delete");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeeting(CreateMeetingCommand command)
        {
            try
            {
                _logger.LogInformation("Create meetind with");
                var meetingId = await _mediator.Send(command);

                return Created(nameof(GetById), meetingId);
            }
            catch (Exception ex )
            {
                _logger.LogError(ex, "Error occurred while creating meeting");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally 
            {
                _logger.LogInformation("Finished Create");
            }
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecenteMeeting([FromQuery] int? number = null)
        {
            try
            {
                _logger.LogInformation($"Get recent meeting with {number} page");

                var result = await _mediator.Send(new GetRecentMeetingQuery(number));
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Meeting not found");
                return NotFound("Meeting not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting meeting");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished Get recent meeting");
            }
        }

        [HttpGet("up-coming")]
        public async Task<IActionResult> GetUpComingMeeting([FromQuery] int? number = null)
        {
            try
            {
                _logger.LogInformation($"Get up coming meeting with {number} page");

                var result = await _mediator.Send(new GetUpComingMeetingQuery(number));
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Meeting not found");
                return NotFound("Meeting not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting meeting");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished Get become meeting");
            }
        }
    }
}
