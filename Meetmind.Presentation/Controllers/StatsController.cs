using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.QueryHandles.Meetings;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly ILogger<StatsController> _logger;
        public StatsController(ISender sender, ILogger<StatsController> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        [HttpGet("storage")]
        public IActionResult GetStorageStats()
        {
            try
            {
                _logger.LogInformation("GetStorageStats called");
                var dbPath = Path.Combine(AppContext.BaseDirectory, "Data/meetmind.db");
                var dbSizeBytes = new FileInfo(dbPath).Length;

                var audioDir = Path.Combine(AppContext.BaseDirectory, "Resources/audio");
                long audioSizeBytes = 0;
                if (Directory.Exists(audioDir))
                {
                    audioSizeBytes = Directory.EnumerateFiles(audioDir, "*", SearchOption.AllDirectories)
                        .Sum(f => new FileInfo(f).Length);
                }

                // 3. Taille totale MeetMind
                var meetmindBytes = dbSizeBytes + audioSizeBytes;
                var meetmindGB = meetmindBytes / (1024.0 * 1024.0 * 1024.0);
                var meetmindMB = meetmindBytes / (1024.0 * 1024.0);

                // 4. Infos disque réel
                var root = Path.GetPathRoot(audioDir) ?? Path.GetPathRoot(AppContext.BaseDirectory);
                var drive = new DriveInfo(root ?? "/");

                var totalDiskGB = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                var freeDiskGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                var usedDiskGB = totalDiskGB - freeDiskGB;

                var usagePercent = totalDiskGB > 0 ? (meetmindGB / totalDiskGB) * 100.0 : 0.0;

                // 5. Construction DTO
                var dto = new StorageStatsDto
                {
                    UsedGB = Math.Round(meetmindGB, 2),
                    UsedMB = Math.Round(meetmindMB, 2),
                    UsedBytes = meetmindBytes,
                    UsagePercent = Math.Round(usagePercent, 2),
                    DiskUsedGB = Math.Round(usedDiskGB, 2),
                    DiskFreeGB = Math.Round(freeDiskGB, 2),
                    DiskTotalGB = Math.Round(totalDiskGB, 2)
                };

                return Ok(dto);
            }
            catch (Exception ex )
            {
                _logger.LogError(ex, "Error occurred while getting storage stats");
               return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished GetStorageStats");
            }
        }

        [HttpGet("global")]
        public async Task<ActionResult<GlobalStatsDto>> GetGlobalStats()
        {
            try
            {
                _logger.LogInformation("GetGlobalStats called");
                var result = await _sender.Send(new GetStatsQuery());
                if (result == null)
                {
                    _logger.LogWarning("No global stats found");
                    return NotFound("No global stats found");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting global stats");

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
            finally
            {
                _logger.LogInformation("Finished GetGlobalStats");
            }
        }
    }
}
