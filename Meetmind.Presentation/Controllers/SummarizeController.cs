using MediatR;
using Meetmind.Application.Command.Summarize;
using Meetmind.Application.QueryHandles.Summarize;
using Meetmind.Application.Services.Pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace Meetmind.Presentation.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class SummarizeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SummarizeController> _logger;
        public SummarizeController(IMediator mediator, ILogger<SummarizeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        [HttpPost("{id}")]
        public async Task<IActionResult> SummarizeAsync([FromBody] Guid id)
        {
            try
            {
                _logger.LogInformation("SummarizeAsync called with request: {@id}", id);
                var result = await _mediator.Send(new SummarizeCommand(id));
                return Ok(result);
            }
            catch (KeyNotFoundException exk)
            {
                _logger.LogWarning("Meeting not found for summarization");
                return NotFound(exk.Message);
            }
            catch (InvalidOperationException exi)
            {
                _logger.LogWarning("Invalid operation during summarization: {Message}", exi.Message);
                return BadRequest(exi.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while summarizing");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished SummarizeAsync");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSummaryByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("GetSummaryByIdAsync called for meeting with id {Id}", id);
                var summary = await _mediator.Send(new GetSummaryByIdQuery(id));
                return Ok(summary);
            }
            catch (KeyNotFoundException exk)
            {
                _logger.LogWarning("Meeting with id {Id} not found", id);
                return NotFound(exk.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting summary for meeting {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetSummaryPdfAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("GetSummaryPdfAsync called for meeting with id {Id}", id);
                var summarize = await _mediator.Send(new GetSummaryByIdQuery(id));

                var document = new SummaryPdfDocument(summarize);

                var stream = new MemoryStream();
                document.GeneratePdf(stream);
                stream.Seek(0, SeekOrigin.Begin);

                var fileName = $"Summary_{summarize.MeetingTitle ?? "meeting"}_{id}.pdf";

                return File(stream, "application/pdf", $"summary_{id}.pdf");
            }
            catch (KeyNotFoundException exk)
            {
                _logger.LogWarning("Meeting with id {Id} not found for PDF generation", id);
                return NotFound(exk.Message);
            }
            catch (InvalidOperationException exi)
            {
                _logger.LogWarning("Invalid operation during PDF generation: {Message}", exi.Message);
                return BadRequest(exi.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating summary PDF for meeting {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
