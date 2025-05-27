using Meetmind.Application.Dto;
using Microsoft.AspNetCore.SignalR;

namespace Meetmind.Presentation.Hubs
{
    public class MeetingHub : Hub
    {
        public async Task NotifyMeetingCreated(MeetingDto meeting)
         => await Clients.All.SendAsync("MeetingCreated", meeting);

        public async Task NotifyMeetingUpdated(MeetingDto meeting)
            => await Clients.All.SendAsync("MeetingUpdated", meeting);

        public async Task NotifyMeetingDeleted(string id)
            => await Clients.All.SendAsync("MeetingDeleted", id);
    }
}
