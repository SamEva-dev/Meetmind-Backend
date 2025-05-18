using System.Threading;
using MediatR;
using Meetmind.Application.Command.Meetings;
using Meetmind.Application.Command.Recording;
using Meetmind.Application.Connectors;
using Meetmind.Application.Services;
using Meetmind.Application.Workers;
using Meetmind.Domain.Enums;
using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Workers;

public sealed class CalendarWorker : BackgroundService, ICalendarWorker
{
    private readonly ILogger<CalendarWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDateTimeProvider _clock;
    private readonly MeetMindDbContext _db;
    private readonly IMediator _mediator;

    public CalendarWorker(ILogger<CalendarWorker> logger, 
        IServiceScopeFactory scopeFactory, 
        IDateTimeProvider clock,
        IMediator mediator)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _clock = clock;
        _mediator = mediator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CalendarWorker started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var meetingCreator = scope.ServiceProvider.GetRequiredService<IMeetingCreatorService>();

                var now = _clock.UtcNow;
                var meetings = await meetingCreator.GetTodayMeetingsFromCalendarsAsync(now, stoppingToken);

                foreach (var calendarMeeting in meetings)
                {
                    var created = await meetingCreator.CreateMeetingIfNotExistsAsync(calendarMeeting, stoppingToken);
                    if (created)
                    {
                        // notifier immédiatement le frontend de la nouvelle réunion
                        await meetingCreator.NotifyMeetingCreatedAsync(calendarMeeting);
                    }
                }

                // Gérer les alertes à 10min / 5min / toutes les 1min
                await meetingCreator.NotifyImminentMeetingsAsync(stoppingToken);
                await ProcessAutoRecordingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans CalendarWorker : {Message}", ex.Message);
            }
        }
    }

    private async Task ProcessAutoRecordingAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeetMindDbContext>();

        await CancelDeletedOrRemoteCancelledMeetingsAsync(dbContext,cancellationToken);
        await DetectAndHandleGhostMeetingsAsync(dbContext,cancellationToken);
        await ProcessAutoStartAsync(dbContext, cancellationToken);
        await ProcessAutoStopAsync(dbContext, cancellationToken);
    }

    private async Task ProcessAutoStartAsync(MeetMindDbContext dbContext, CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var settings = await dbContext.Settings
        .AsNoTracking()
                .FirstOrDefaultAsync(ct);

        if (!settings.AutoStartRecord)
            return;

        var meetingsToStart = await dbContext.Meetings
            .Where(m => m.State == MeetingState.Pending && m.StartUtc <= now && !m.IsCancelled)
            .ToListAsync(ct);

        foreach (var meeting in meetingsToStart)
        {
            _logger.LogInformation("▶️ Auto-start meeting {Id}", meeting.Id);
            await _mediator.Send(new StartRecordingCommand(meeting.Id), ct);
        }
    }

    private async Task ProcessAutoStopAsync(MeetMindDbContext dbContext, CancellationToken ct)
    {
        var now = _clock.UtcNow;

        var meetingsToStop = await dbContext.Meetings
            .AsNoTracking()
            .Where(m =>
                (m.State == MeetingState.Recording || m.State == MeetingState.Paused)
                && m.EndUtc.HasValue && m.EndUtc.Value <= now)
            .ToListAsync(ct);

        foreach (var meeting in meetingsToStop)
        {
            _logger.LogInformation("⏹️ Auto-stop meeting {Id}", meeting.Id);
            await _mediator.Send(new StopRecordingCommand(meeting.Id, DateTime.UtcNow), ct);
        }
    }

    private async Task CancelDeletedOrRemoteCancelledMeetingsAsync(MeetMindDbContext dbContext, CancellationToken ct)
    {
        var meetings = await dbContext.Meetings
       .Where(m => !m.IsCancelled && m.ExternalId != null)
       .ToListAsync(ct);
        using var scope = _scopeFactory.CreateScope();
        var _calendarConnectors = scope.ServiceProvider.GetRequiredService<IEnumerable<ICalendarConnector>>();
        foreach (var meeting in meetings)
        {
            var connector = _calendarConnectors.FirstOrDefault(c => c.Source == meeting.ExternalSource);
            if (connector == null) continue;

            var isCancelled = await connector.IsCancelledAsync(meeting.ExternalId!, ct);
            if (isCancelled)
            {
                _logger.LogWarning("🗑️ Réunion annulée distante détectée {Id}", meeting.Id);
                meeting.Cancel();
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }

    private async Task DetectAndHandleGhostMeetingsAsync(MeetMindDbContext dbContext, CancellationToken ct)
    {
        var now = _clock.UtcNow;

        var ghostMeetings = await dbContext.Meetings
            .Where(m => m.State == MeetingState.Pending && m.EndUtc.HasValue && m.EndUtc.Value <= now)
            .ToListAsync(ct);

        foreach (var meeting in ghostMeetings)
        {
            _logger.LogWarning("🕸️ Réunion fantôme détectée {Id}, annulation automatique.", meeting.Id);
            meeting.Cancel();
        }

        await dbContext.SaveChangesAsync(ct);
    }

}
