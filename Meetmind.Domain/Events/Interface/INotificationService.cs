
using Meetmind.Domain.Entities;

namespace Meetmind.Domain.Events.Interface;

 public interface INotificationService
{
    Task NotifySettingsUpdatedAsync(SettingsEntity settings);
    Task SendNewMeeting(MeetingEntity meeting);
    Task SendReminderMeeting(Guid meetingId, string message);

    Task NotifyMeetingStarted(Guid meetingId);
    Task NotifyMeetingPaused(Guid meetingId);
    Task NotifyMeetingResumed(Guid meetingId);
    Task NotifyMeetingStopped(Guid meetingId);
}
