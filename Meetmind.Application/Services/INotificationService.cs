using Meetmind.Application.Dto;

namespace Meetmind.Application.Services
{
    public interface INotificationService
    {
        Task NotifySettingsUpdatedAsync(SettingsDto dto);
    }
}
