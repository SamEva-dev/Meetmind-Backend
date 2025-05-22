using Meetmind.Application.Services;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;
using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Workers;

public sealed class TranscriptionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TranscriptionWorker> _logger;

    public TranscriptionWorker(IServiceScopeFactory scopeFactory,
        ILogger<TranscriptionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessTranscriptionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans TranscriptionWorker");
            }

            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
    }

    private async Task ProcessTranscriptionsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MeetMindDbContext>();
        var _transcriptionService = scope.ServiceProvider.GetRequiredService<ITranscriptionService>();

        var test = await db.Meetings.ToListAsync(ct);
        var meetingsToTranscribe = await db.Meetings
            .Where(m => m.State == MeetingState.Done && m.TranscriptState == TranscriptState.Queued)
            .ToListAsync(ct);

        foreach (var meeting in meetingsToTranscribe)
        {
            try
            {
                meeting.MarkTranscriptionProcessing();
                await db.SaveChangesAsync(ct);

                // 💬 Simulation traitement transcription
                var transcriptPath = await _transcriptionService.TranscribeAsync(meeting, ct);

                meeting.MarkTranscriptionCompleted();
                _logger.LogInformation("✅ Transcription complétée pour {Id}", meeting.Id);
            }
            catch (Exception ex)
            {
                meeting.MarkTranscriptionFailed();
                _logger.LogError(ex, "❌ Échec de la transcription pour {Id}", meeting.Id);
            }

            await db.SaveChangesAsync(ct);
        }
    }

    // Simulation du traitement (remplacer par appel gRPC ou Whisper plus tard)
    private Task<string> SimulateTranscriptionAsync(MeetingEntity meeting, CancellationToken ct)
    {
        var path = $"transcripts/{meeting.Id}.txt";
        File.WriteAllText(path, $"Transcription simulée pour la réunion {meeting.Title} à {DateTime.UtcNow}");
        return Task.FromResult(path);
    }
}
