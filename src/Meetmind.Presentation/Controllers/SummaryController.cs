using MediatR;
using Meetmind.Application.Commands;
using Meetmind.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers;

[ApiController]
[Route("v1/meetings/{id}/summary")]
public class SummaryController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<SummaryController> _logger;

    public SummaryController(ISender sender, ILogger<SummaryController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Trigger(Guid id)
    {
        try
        {
            await _sender.Send(new TriggerSummaryCommand(id));
            return Accepted();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var markdown = await _sender.Send(new GetSummaryQuery(id));
            return Content(markdown, "text/markdown");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (FileNotFoundException)
        {
            return StatusCode(500, "Summary file missing.");
        }
        catch (InvalidOperationException)
        {
            return StatusCode(425, "Summary not ready.");
        }
    }
}