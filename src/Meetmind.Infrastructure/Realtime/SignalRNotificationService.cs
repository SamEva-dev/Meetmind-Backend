using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Models.Realtime;
using Meetmind.Domain.Models;
using Meetmind.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Realtime;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotifyHub> _hub;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(IHubContext<NotifyHub> hub, ILogger<SignalRNotificationService> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public Task NotifyUpcomingAsync(UpcomingMeeting meeting, int minutesBefore, CancellationToken ct)
    {
        var payload = new UpcomingMeetingNotification
        {
            MeetingId = meeting.Id,
            Title = meeting.Title,
            StartUtc = meeting.StartUtc,
            MinutesBefore = minutesBefore,
            Source = meeting.Source
        };

        _logger.LogInformation("SignalR: NotifyUpcoming -> {Title} in {Min}min", meeting.Title, minutesBefore);
        return _hub.Clients.All.SendAsync("UpcomingMeeting", payload, ct);
    }

    public Task RequestUserConfirmationAsync(Guid meetingId, string action, CancellationToken ct)
    {
        var message = new RequestActionMessage
        {
            MeetingId = meetingId,
            Action = action,
            Label = $"Do you want to {action.Replace('_', ' ')}?",
            TriggeredAtUtc = DateTime.UtcNow
        };

        _logger.LogInformation("SignalR: RequestAction -> {Action} for Meeting {Id}", action, meetingId);
        return _hub.Clients.All.SendAsync("RequestAction", message, ct);
    }

    public Task NotifyActionResultAsync(ActionResultMessage message, CancellationToken ct)
    {
        _logger.LogInformation("SignalR: ActionResult -> {Action} = {Status}", message.Action, message.Status);
        return _hub.Clients.All.SendAsync("ActionResult", message, ct);
    }
}