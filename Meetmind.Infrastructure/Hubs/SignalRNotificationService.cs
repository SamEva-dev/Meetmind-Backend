using Meetmind.Application.Dto;
using Meetmind.Application.Services;
using Meetmind.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Meetmind.Infrastructure.Hubs
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<SettingsHub> _settingsHub;

        public SignalRNotificationService(
        IHubContext<SettingsHub> settingsHub)
        {
            _settingsHub = settingsHub;
        }

        public async Task NotifySettingsUpdatedAsync(SettingsDto dto)
        {
            await _settingsHub.Clients.All.SendAsync("SettingsUpdated", dto);
        }
    }
}
