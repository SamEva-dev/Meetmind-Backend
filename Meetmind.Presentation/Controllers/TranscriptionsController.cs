using MediatR;
using Meetmind.Application.Command.Transcription;
using Meetmind.Application.QueryHandles.Transcription;
using Meetmind.Application.Services.Pdf;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace Meetmind.Presentation.Controllers
{
    [Route("v1/meetings/")]
    [ApiController]
    public class TranscriptionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TranscriptionsController> _logger;

        public TranscriptionsController(IMediator mediator, ILogger<TranscriptionsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("transcript")]
        public async Task<IActionResult> GetTranscription()
        {
            try
            {
                _logger.LogInformation("GetTranscription called");
                var result = await _mediator.Send(new GetTranscriptionQuery());
                return Ok(result);
            }
            catch (KeyNotFoundException exk)
            {
                _logger.LogWarning("Meeting not found for transcription");
                return NotFound(exk.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting transcription ");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished GetTranscription");
            }
        }

        [HttpGet("{id}/transcript")]
        public async Task<IActionResult> GetTranscriptionById(Guid id)
        {
            try
            {
                _logger.LogInformation("GetTranscription called for meeting with id {Id}", id);
                var result = await _mediator.Send(new GetTranscriptionByIdQuery(id));
                return Ok(result);
            }
            catch (KeyNotFoundException exk)
            {
                _logger.LogWarning("Meeting with id {Id} not found", id);
                return NotFound(exk.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting transcription for meeting with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished GetTranscription");
            }
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetTranscriptPdf(Guid id)
        {
            try
            {
                _logger.LogInformation("GetTranscriptPdf called for meeting with id {Id}", id);
                var transcript = await _mediator.Send(new GetTranscriptionByIdQuery(id));

                var doc = new TranscriptPdfDocument(transcript);
                var stream = new MemoryStream();

                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
                doc.GeneratePdf(stream);
                stream.Position = 0;

                return File(stream, "application/pdf", $"Transcription_{id}.pdf");
            }
            catch (KeyNotFoundException exk)
            {
                _logger.LogWarning("Meeting with id {Id} not found", id);
                return NotFound(exk.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting PDF for meeting with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished GetTranscriptPdf");
            }
        }

        [HttpPost("{id}/transcript")]
        public async Task<IActionResult> RequestTranscription(Guid id)
        {
            try
            {
                _logger.LogInformation("Requesting transcription for meeting with id {Id}", id);
                await _mediator.Send(new TranscriptionCommand(id));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Meeting with id {Id} not found", id);
                return NotFound("Meeting not found.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while requesting transcription for meeting with id {Id}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while requesting transcription for meeting with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished RequestTranscription");
            }
        }

        [HttpDelete("{id}/transcript")]
        public async Task<IActionResult> DeleteTranscription(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting transcription for meeting with id {Id}", id);
                await _mediator.Send(new DeleteTranscriptionCommand(id));
                return NoContent();
            }
            catch (KeyNotFoundException exk)
            {
                _logger.LogWarning("Meeting with id {Id} not found", id);
                return NotFound(exk.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting transcription for meeting with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished DeleteTranscription");
            }
        }
    }
}
