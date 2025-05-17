using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.QueryHandles.Mettings;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("api/[controller]")]
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
            catch (Exception)
            {
                _logger.LogError("Error occurred while getting today's meetings");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished GetToday");
            }

        }
    }
}
