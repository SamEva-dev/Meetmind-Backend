using Meetmind.Application.Services;
using Meetmind.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Services.Summarize;

public class SummarizeService : ISummarizeService
{
    private readonly ILogger<SummarizeService> _logger;
    private readonly AudioSummarizeService _audioSummarizeService;

    public SummarizeService(ILogger<SummarizeService> logger, AudioSummarizeService audioSummarizeService)
    {
        _logger = logger;
        _audioSummarizeService = audioSummarizeService;
    }

    public async Task<string> SummarizeAsync(MeetingEntity meeting, CancellationToken ct)
    {
        _logger.LogInformation("Starting summarization for meeting {MeetingId}", meeting.Id);
        
        try
        {
            var summaryPath = await _audioSummarizeService.SummarizeAsync(meeting, ct);
            _logger.LogInformation("Summarization completed for meeting {MeetingId}, summary path: {SummaryPath}", meeting.Id, summaryPath);
            return summaryPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during summarization for meeting {MeetingId}", meeting.Id);
            throw;
        }
    }
}
