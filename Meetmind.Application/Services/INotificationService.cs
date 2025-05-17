using Meetmind.Application.Dto;

namespace Meetmind.Application.Services
{
    public interface INotificationService
    {
        Task NotifySettingsUpdatedAsync(SettingsDto dto);
        Task SendNewMeeting(MeetingDto meeting);
        Task SendReminderMeeting(Guid meetingId, string message);
    }
}
