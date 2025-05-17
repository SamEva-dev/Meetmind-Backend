using Meetmind.Application.Dto;
using Meetmind.Application.Services;
using Meetmind.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Meetmind.Infrastructure.Hubs
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<SettingsHub> _settingsHub;
        private readonly IHubContext<MeetingHub> _meetingsHub;
        public SignalRNotificationService(
        IHubContext<SettingsHub> settingsHub)
        {
            _settingsHub = settingsHub;
        }

        public async Task NotifySettingsUpdatedAsync(SettingsDto dto)
        {
            await _settingsHub.Clients.All.SendAsync("SettingsUpdated", dto);
        }

        public async Task SendNewMeeting(MeetingDto meeting) =>
        await _meetingsHub.Clients.All.SendAsync("NewMeeting", meeting);

        public async Task SendReminderMeeting(Guid meetingId, string message) =>
            await _meetingsHub.Clients.All.SendAsync("MeetingReminder", new { meetingId, message });
    }
}
