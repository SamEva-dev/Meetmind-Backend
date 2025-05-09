using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Enums;
using Meetmind.Domain.Models.Realtime;
using Meetmind.Domain.Models;
using Microsoft.Extensions.Logging;
using Meetmind.Domain.ValueObjects;

namespace Meetmind.Infrastructure.Orchestration;

public class MeetingOrchestrator
{
    private readonly INotificationService _notifier;
    private readonly ILogger<MeetingOrchestrator> _logger;

    private MeetingExecutionState _state = MeetingExecutionState.Scheduled;
    private readonly UpcomingMeeting _meeting;
    private readonly UserSettings _settings;

    public Guid Id => _meeting.Id;
    public MeetingExecutionState State => _state;

    public MeetingOrchestrator(
        UpcomingMeeting meeting,
        UserSettings settings,
        INotificationService notifier,
        ILogger<MeetingOrchestrator> logger)
    {
        _meeting = meeting;
        _settings = settings;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task NotifyUpcomingAsync(int minBefore, CancellationToken ct)
    {
        _logger.LogInformation("Notify: {Title} in {Min} min", _meeting.Title, minBefore);
        await _notifier.NotifyUpcomingAsync(_meeting, minBefore, ct);
    }

    public async Task HandleStartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Meeting {Id} starts now", _meeting.Id);

        if (_settings.AutoStartRecord)
        {
            _logger.LogInformation("AutoStart enabled → starting recording");
            _state = MeetingExecutionState.Recording;
            // 👉 lancer ici IRecordingService.StartAsync(...)
        }
        else
        {
            _logger.LogInformation("AutoStart disabled → request UI confirmation");
            _state = MeetingExecutionState.WaitingUserConfirmation;

            await _notifier.RequestUserConfirmationAsync(_meeting.Id, "start_record", ct);
        }
    }

    public async Task HandleUserConfirmation(ConfirmActionMessage msg, CancellationToken ct)
    {
        if (_state != MeetingExecutionState.WaitingUserConfirmation)
        {
            _logger.LogWarning("Unexpected user confirmation in state {State}", _state);
            return;
        }

        if (msg.Accepted)
        {
            _logger.LogInformation("User confirmed recording → starting");
            _state = MeetingExecutionState.Recording;
            // 👉 déclencher IRecordingService.StartAsync(...)
        }
        else
        {
            _logger.LogInformation("User declined → cancelling meeting");
            _state = MeetingExecutionState.Cancelled;
        }

        await _notifier.NotifyActionResultAsync(new ActionResultMessage
        {
            MeetingId = _meeting.Id,
            Action = "start_record",
            Status = msg.Accepted ? "success" : "cancelled",
            Message = msg.Accepted ? "Recording started" : "Recording cancelled"
        }, ct);
    }
}