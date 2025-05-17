using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.QueryHandles.Logs;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("api/sync/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly ISender _mediator;

        public LogsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<CalendarSyncLogDto>>> GetLogs()
        {
            var result = await _mediator.Send(new GetSyncLogsQuery());
            return Ok(result);
        }
    }
}
