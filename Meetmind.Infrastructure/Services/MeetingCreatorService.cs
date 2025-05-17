
using System.Threading;
using Meetmind.Application.Connectors;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;
using Meetmind.Domain.Models;
using Meetmind.Infrastructure.Database;
using Meetmind.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Services;

public sealed class MeetingCreatorService : IMeetingCreatorService
{
    private readonly IEnumerable<ICalendarConnector> _calendarConnectors;
    private readonly IDateTimeProvider _clock;
    private readonly IHubContext<MeetingHub> _hub;
    private readonly MeetMindDbContext _db;
    private readonly ILogger<MeetingCreatorService> _logger;

    public MeetingCreatorService(
        IEnumerable<ICalendarConnector> calendarConnectors,
        IDateTimeProvider clock,
        IHubContext<MeetingHub> hub,
        MeetMindDbContext db,
        ILogger<MeetingCreatorService> logger)
    {
        _calendarConnectors = calendarConnectors;
        _clock = clock;
        _hub = hub;
        _db = db;
        _logger = logger;
    }

    public async Task<List<CalendarMeetingDto>> GetTodayMeetingsFromCalendarsAsync(DateTime utcNow, CancellationToken token)
    {
        try
        {
            var allMeetings = new List<CalendarMeetingDto>();

            foreach (var connector in _calendarConnectors)
            {
                try
                {
                    var events = await connector.GetTodayMeetingsAsync(token);
                    allMeetings.AddRange(events);

                    _db.CalendarSyncLogs.Add(new CalendarSyncLog
                    {
                        TimestampUtc = _clock.UtcNow,
                        Source = connector.GetType().Name,
                        TotalEventsFound = events.Count,
                        MeetingsCreated = 0
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while syncing from {Source}", connector.GetType().Name);

                    _db.CalendarSyncLogs.Add(new CalendarSyncLog
                    {
                        TimestampUtc = _clock.UtcNow,
                        Source = connector.GetType().Name,
                        TotalEventsFound = 0,
                        MeetingsCreated = 0,
                        ErrorMessage = ex.Message
                    });
                }
            }

            await _db.SaveChangesAsync(token);
            return allMeetings;
        }
        catch (Exception ex)
        {
            return new List<CalendarMeetingDto>();
        }
        
    }

    public async Task<bool> CreateMeetingIfNotExistsAsync(CalendarMeetingDto dto, CancellationToken token)
    {
        var exists = await _db.Meetings
            .AnyAsync(m => m.ExternalId == dto.ExternalId && m.ExternalSource == dto.Source, token);

        if (exists) return false;

        var meeting = new MeetingEntity(dto.Title, dto.StartUtc, dto.ExternalId, dto.Source);
        _db.Meetings.Add(meeting);

        // projection read model
        _db.MeetingReadModels.Add(new MeetingReadModel
        {
            Id = meeting.Id,
            Title = meeting.Title,
            StartUtc = meeting.StartUtc,
            EndUtc = meeting.EndUtc,
            State = meeting.State.ToString(),
            TranscriptPath = null,
            SummaryPath = null,
            ExternalId = dto.ExternalId,
            ExternalSource = dto.Source
        });

        // mise à jour du log en mémoire
        var lastLog = _db.CalendarSyncLogs
            .Local
            .Where(l => l.Source == dto.Source)
            .OrderByDescending(l => l.TimestampUtc)
            .FirstOrDefault();

        if (lastLog != null)
            lastLog.MeetingsCreated++;

        await _db.SaveChangesAsync(token);
        return true;
    }

    public async Task NotifyMeetingCreatedAsync(CalendarMeetingDto dto)
    {
        await _hub.Clients.All.SendAsync("NewMeeting", dto);
    }

    public async Task NotifyImminentMeetingsAsync(CancellationToken token)
    {
        var now = _clock.UtcNow;
        var today = now.Date;

        var meetings = await _db.Meetings
            .AsNoTracking()
            .Where(m => m.StartUtc.Date == today && m.State == MeetingState.Pending)
            .ToListAsync(token);
        var setting = await _db.Settings
                .AsNoTracking()
                .FirstOrDefaultAsync(token);
        var notifyBeforeMinutes = setting?.NotifyBeforeMinutes ?? 10;
        var notificationRepeatInterval = setting?.NotificationRepeatInterval ?? 1;

        foreach (var meeting in meetings)
        {
            var minutesLeft = (meeting.StartUtc - now).TotalMinutes;

            if (minutesLeft <= notifyBeforeMinutes)
                await SendReminder(meeting.Id, $"La réunion commence dans {notifyBeforeMinutes} minutes");

            else if (minutesLeft <= notificationRepeatInterval && minutesLeft > 1)
                await SendReminder(meeting.Id, $"La réunion commence dans {notificationRepeatInterval} minutes");

            else if (minutesLeft <= 1 && minutesLeft > 0)
                await SendReminder(meeting.Id, "La réunion commence dans une minute !");
        }
    }

    private async Task SendReminder(Guid meetingId, string message)
    {
        await _hub.Clients.All.SendAsync("MeetingReminder", new
        {
            MeetingId = meetingId,
            Message = message
        });
    }
}