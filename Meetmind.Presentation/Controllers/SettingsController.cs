using System.ComponentModel.DataAnnotations;
using MediatR;
using Meetmind.Application.Command.Settings;
using Meetmind.Application.QueryHandles.Settings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ISender sender, ILogger<SettingsController> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            _logger.LogInformation("GetSettings called");
            try
            {
                var result = await _sender.Send(new GetSettingsQuery());

                return Ok(result);
            }
            catch (KeyNotFoundException exk)
            {
                _logger.LogWarning("Settings not found ");
                return NotFound(exk.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting settings");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished GetSettings ");
            }   

        }

        [HttpPost]
        public async Task<IActionResult> CreateSetting(SettingsCommand command)
        {
            try
            {
                _logger.LogInformation("CreateSetting called with command: {@Command}", command);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var result = await _sender.Send(command);
                return CreatedAtAction(nameof(GetSettings), result);
            }
           
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating settings");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished CreateSetting");
            }
        }
    }
}
