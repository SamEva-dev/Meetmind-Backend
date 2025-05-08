using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers;

[Route("v1/meetings")]
[ApiController]
public class MeetingsController : ControllerBase
{
    private readonly ISender _sender;

    public MeetingsController(ISender sender) => _sender = sender;

    [HttpPost("{id}/recording/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        //await _sender.Send(new StartMeetingCommand(id));
        return NoContent();
    }

    [HttpPost("{id}/recording/pause")]
    public async Task<IActionResult> Pause(Guid id)
    {
        //await _sender.Send(new PauseMeetingCommand(id));
        return NoContent();
    }

    [HttpPost("{id}/recording/resume")]
    public async Task<IActionResult> Resume(Guid id)
    {
        //await _sender.Send(new ResumeMeetingCommand(id));
        return NoContent();
    }

    [HttpPost("{id}/recording/stop")]
    public async Task<IActionResult> Stop(Guid id)
    {
        var now = DateTime.UtcNow;
        //await _sender.Send(new StopMeetingCommand(id, now));
        return NoContent();
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        //var result = await _sender.Send(new GetMeetingsTodayQuery());
        return Ok();
    }
}