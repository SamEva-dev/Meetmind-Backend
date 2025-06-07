using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Models;

namespace Meetmind.Application.Services.Notification;

 public interface INotificationService
{
    Task SendReminderMeeting(Guid meetingId, string message);

    Task NotifyMeetingAsync(Notifications notifications, CancellationToken cancellationToken);

    Task NotifyRecordingErrorAsync(Guid meetingId, string error, CancellationToken cancellationToken);
    Task NotifySettingsUpdatedAsync(SettingsEntity settings, CancellationToken cancellationToken);
    Task NotifyMeetingCreatedAsync(MeetingDto meetingDto, CancellationToken cancellationToken);
    Task NotifyMeetingDeletedAsync(Guid meeting, CancellationToken cancellationToken);
    Task NotifyMeetingCancelledAsync(Guid id, CancellationToken cancellationToken);
    Task NotifyRecordingStartedAsync(MeetingDto meetingDto, CancellationToken cancellationToken);
    Task NotifyRecordingStoppedAsync(MeetingDto meetingDto, CancellationToken cancellationToken);
    Task NotifyRecordingResumedAsync(MeetingDto meetingDto, CancellationToken cancellationToken);
    Task NotifyRecordingPausedAsync(MeetingDto meetingDto, CancellationToken ct);
    Task NotifyFragmentUploadedAsync(Guid meetingId, int sequenceNumber, CancellationToken cancellationToken);
    Task NotifyAudioDeletedAsync(MeetingDto meetingDto, CancellationToken cancellationToken);
    Task NotifySummaryQueuedAsync(MeetingDto meetingDto, CancellationToken cancellationToken);
    Task NotifyTranscriptionDeletedAsync(CancellationToken cancellationToken);
    Task NotifyTranscriptionQueuedAsync(MeetingDto meetingDto, CancellationToken cancellationToken);
    Task NotifySummaryProcessingAsync(MeetingDto meetingDto, CancellationToken ct);
    Task NotifySummaryCompletedAsync(MeetingDto meetingDto, CancellationToken ct);
    Task NotifySummaryFailedAsync(MeetingDto meetingDto, CancellationToken ct);
    Task NotifyTranscriptionProcessingAsync(MeetingDto meetingDto, CancellationToken ct);
    Task NotifyTranscriptionCompletedAsync(MeetingDto meetingDto, CancellationToken ct);
    Task NotifyTranscriptionErrorAsync(MeetingDto meetingDto, string message, CancellationToken ct);
    Task NotifyLiveTranscriptionAsync(string text, CancellationToken ct);
}
