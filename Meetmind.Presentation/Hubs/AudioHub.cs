using Microsoft.AspNetCore.SignalR;

namespace Meetmind.Presentation.Hubs;

public class AudioHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var meetingId = Context.GetHttpContext().Request.Query["meetingId"];
        if (!string.IsNullOrEmpty(meetingId))
            await Groups.AddToGroupAsync(Context.ConnectionId, meetingId);
        await base.OnConnectedAsync();
    }
}
