using Meetmind.Application.Common.Interfaces;
using Meetmind.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Meetmind.Infrastructure.Hubs;

public class NotifyHubNotifier : INotifier
{
    private readonly IHubContext<NotifyHub> _hub;

    public NotifyHubNotifier(IHubContext<NotifyHub> hub)
    {
        _hub = hub;
    }

    public Task BroadcastMeetingStateAsync(Guid meetingId, string newState, CancellationToken ct)
    {
        return _hub.Clients.All.SendAsync("MeetingStateChanged", new
        {
            MeetingId = meetingId,
            NewState = newState
        }, ct);
    }
}