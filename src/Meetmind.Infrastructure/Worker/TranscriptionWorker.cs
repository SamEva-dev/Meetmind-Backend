using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;
using Meetmind.Infrastructure.Db;
using Meetmind.Infrastructure.Transcription;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Worker;

public class TranscriptionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TranscriptionWorker> _logger;
    private const int IntervalSeconds = 10;
    private readonly TranscriptSemanticAnalyzer _analyzer;

    public TranscriptionWorker(IServiceScopeFactory scopeFactory, ILogger<TranscriptionWorker> logger, TranscriptSemanticAnalyzer analyzer)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[{Time}] Transcription worker started.", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MeetMindDbContext>();
                var whisper = scope.ServiceProvider.GetRequiredService<IWhisperService>();

                var pending = await db.Meetings
                    .Where(m => m.TranscriptState == TranscriptState.Queued)
                    .ToListAsync(stoppingToken);

                foreach (var meeting in pending)
                {
                    try
                    {
                        _logger.LogInformation("[{Time}] Processing transcription for Meeting {Id}.",
                            DateTime.UtcNow, meeting.Id);

                        meeting.MarkTranscriptionProcessing();
                        await db.SaveChangesAsync(stoppingToken);

                        var transcriptPath = await whisper.TranscribeAsync(meeting.Id, stoppingToken);

                        var path = await _analyzer.AnalyzeAsync(meeting.Id, transcriptPath, stoppingToken);
                        _logger.LogInformation("Semantic analysis saved to {Path}", path);

                        meeting.MarkTranscriptionCompleted(transcriptPath);
                        await db.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("[{Time}] Transcription completed for Meeting {Id}.",
                            DateTime.UtcNow, meeting.Id);
                    }
                    catch (Exception ex)
                    {
                        meeting.MarkTranscriptionFailed();
                        await db.SaveChangesAsync(stoppingToken);

                        _logger.LogError(ex, "[{Time}] Transcription failed for Meeting {Id}.",
                            DateTime.UtcNow, meeting.Id);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(IntervalSeconds), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "[{Time}] Transcription worker crashed.");
                await Task.Delay(5000, stoppingToken); // Attendre avant retry
            }
        }
    }
}