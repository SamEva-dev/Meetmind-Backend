using AutoMapper;
using Meetmind.Application.Dto;
using Meetmind.Application.Services;
using Meetmind.Application.Services.Notification;
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
    private readonly IMapper _mapper;

    public TranscriptionWorker(
        IServiceScopeFactory scopeFactory,
        IMapper mapper,
        ILogger<TranscriptionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _mapper = mapper;
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
        var _notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
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
                await _notificationService.NotifyTranscriptionProcessingAsync(_mapper.Map<MeetingDto>(meeting), ct);
                await _notificationService.NotifyMeetingAsync(new Domain.Models.Notifications
                {
                    MeetingId = meeting.Id,
                    Title = meeting.Title,
                    Message = $"Transcription en cours pour la réunion {meeting.Id}",
                    Time = DateTime.UtcNow
                }, ct);
                await db.SaveChangesAsync(ct);

                await _transcriptionService.TranscribeAsync(meeting, ct);

                meeting.MarkTranscriptionCompleted();
                meeting.QueueSummary();
                await _notificationService.NotifyTranscriptionCompletedAsync(_mapper.Map<MeetingDto>(meeting), ct);
                await _notificationService.NotifyMeetingAsync(new Domain.Models.Notifications
                {
                    MeetingId = meeting.Id,
                    Title = meeting.Title,
                    Message = $"Transcription terminée pour la réunion {meeting.Id}",
                    Time = DateTime.UtcNow
                }, ct);
                _logger.LogInformation("✅ Transcription complétée pour {Id}", meeting.Id);
            }
            catch (Exception ex)
            {
                meeting.MarkTranscriptionFailed();
                await _notificationService.NotifyTranscriptionErrorAsync(_mapper.Map<MeetingDto>(meeting), ex.Message, ct);
                _logger.LogError(ex, "❌ Échec de la transcription pour {Id}", meeting.Id);
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
