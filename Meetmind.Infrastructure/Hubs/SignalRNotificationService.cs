using AutoMapper;
using Meetmind.Application.Dto;
using Meetmind.Application.Services.Notification;
using Meetmind.Domain.Entities;
using Meetmind.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Meetmind.Infrastructure.Hubs
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<SettingsHub> _settingsHub;
        private readonly IHubContext<MeetingHub> _meetingsHub;
        private readonly IHubContext<RecordingHub> _recordingHub;
        private readonly IHubContext<AudioHub> _audioHub;
        private readonly IHubContext<TranscriptHub> _transcriptHub;
        private readonly IHubContext<SummaryHub> _summaryHub;
        private readonly IMapper _mapper;

        public SignalRNotificationService(
            IHubContext<SettingsHub> settingsHub, 
            IHubContext<MeetingHub> meetingsHub,
            IHubContext<AudioHub> audioHub,
            IHubContext<RecordingHub> recordingHub,
            IHubContext<TranscriptHub> transcriptHub,
            IHubContext<SummaryHub> summaryHub,
            IMapper mapper)
        {
            _settingsHub = settingsHub;
            _meetingsHub = meetingsHub;
            _audioHub = audioHub;
            _mapper = mapper;
            _recordingHub = recordingHub;
            _transcriptHub = transcriptHub;
            _summaryHub = summaryHub;
        }

        public async Task NotifyAudioDeletedAsync(MeetingDto meetingDto, CancellationToken cancellationToken)
        {
            await _audioHub.Clients.All.SendAsync("AudioDeleted", meetingDto);
        }

        public async Task NotifyFragmentUploaded(Guid meetingId, int sequenceNumber)
        {
            await _meetingsHub.Clients.All.SendAsync("SteamAudio", new
            {
                MeetingId = meetingId,
                SequenceNumber = sequenceNumber
            });
        }

        public async Task NotifyFragmentUploadedAsync(Guid meetingId, int sequenceNumber, CancellationToken cancellationToken)
        {
            await _meetingsHub.Clients.All.SendAsync("FragmentUploaded", new
            {
                MeetingId = meetingId,
                SequenceNumber = sequenceNumber
            }, cancellationToken);
        }

        public async Task NotifyMeetingCancelledAsync(Guid id, CancellationToken cancellationToken)
        {
            await _meetingsHub.Clients.All.SendAsync("MeetingCancelled", id, cancellationToken);
        }

        public async Task NotifyMeetingCreatedAsync(MeetingDto meetingDto, CancellationToken cancellationToken)
        {
            await _meetingsHub.Clients.All.SendAsync("MeetingCreated", meetingDto);
        }

        public async Task NotifyMeetingDeletedAsync(Guid meetingId, CancellationToken cancellationToken)
        {
           await  _meetingsHub.Clients.All.SendAsync("MeetingDeleted", meetingId);
        }

        public async Task NotifyRecordingErrorAsync(Guid meetingId, string error, CancellationToken cancellationToken)
        {
            await _recordingHub.Clients.All.SendAsync("RecordingError", meetingId, error, cancellationToken);
        }

        public async Task NotifyRecordingPausedAsync(MeetingDto meetingDto, CancellationToken ct)
        {
            await _recordingHub.Clients.All.SendAsync("RecordingPaused", meetingDto, ct);
        }

        public async Task NotifyRecordingResumedAsync(MeetingDto meetingDto, CancellationToken cancellationToken)
        {
            await _recordingHub.Clients.All.SendAsync("RecordingResumed", meetingDto, cancellationToken);
        }

        public async Task NotifyRecordingStartedAsync(MeetingDto meetingDto, CancellationToken cancellationToken)
        {
            await _recordingHub.Clients.All.SendAsync("RecordingStarted", meetingDto, cancellationToken);
        }

        public async Task NotifyRecordingStoppedAsync(MeetingDto meetingDto, CancellationToken cancellationToken)
        {
            await _recordingHub.Clients.All.SendAsync("RecordingStopped", meetingDto);
        }

        public async Task NotifySettingsUpdatedAsync(SettingsEntity settings)
        {
            await _settingsHub.Clients.All.SendAsync("SettingsUpdated", _mapper.Map<SettingsDto> (settings));
        }

        public async Task NotifySettingsUpdatedAsync(SettingsEntity settings, CancellationToken cancellationToken)
        {
            await _settingsHub.Clients.All.SendAsync("SettingsUpdated", _mapper.Map<SettingsDto>(settings), cancellationToken);
        }

        public async Task NotifySummaryCompletedAsync(MeetingDto meetingDto, CancellationToken ct)
        {
            await _summaryHub.Clients.All.SendAsync("SummaryCompleted", meetingDto, ct);
        }

        public async Task NotifySummaryFailedAsync(MeetingDto meetingDto, CancellationToken ct)
        {
            await _summaryHub.Clients.All.SendAsync("SummaryFailed", meetingDto, ct);
        }

        public async Task NotifySummaryProcessingAsync(MeetingDto meetingDto, CancellationToken ct)
        {
            await _summaryHub.Clients.All.SendAsync("SummaryProcessing", meetingDto, ct);
        }

        public async Task NotifySummaryQueuedAsync(MeetingDto meetingDto, CancellationToken cancellationToken)
        {
            await _summaryHub.Clients.All.SendAsync("SummaryCreated", meetingDto);
        }

        public async Task NotifyTranscriptionCompletedAsync(MeetingDto meetingDto, CancellationToken ct)
        {
            await _transcriptHub.Clients.All.SendAsync("TranscriptCompleted", meetingDto, ct);
        }

        public async Task NotifyTranscriptionDeletedAsync(CancellationToken cancellationToken)
        {
            await _transcriptHub.Clients.All.SendAsync("TranscripDeleted", cancellationToken);
        }

        public async Task NotifyTranscriptionErrorAsync(MeetingDto meetingDto, string message, CancellationToken ct)
        {
            await _transcriptHub.Clients.All.SendAsync("TranscriptError", meetingDto, message, ct);
        }

        public async Task NotifyTranscriptionQueuedAsync(MeetingDto meetingDto, CancellationToken cancellationToken)
        {
            await _transcriptHub.Clients.All.SendAsync("TranscriptCreated", meetingDto, cancellationToken);
        }

        public async Task NotifyTranscriptionProcessingAsync(MeetingDto meetingDto, CancellationToken ct)
        {
            await _transcriptHub.Clients.All.SendAsync("TranscriptProcessing", meetingDto, ct);
        }

        public async Task SendNewMeeting(MeetingEntity meeting)
        {
            await _meetingsHub.Clients.All.SendAsync("NewMeeting", _mapper.Map<MeetingDto>(meeting));
        }

        public async Task SendReminderMeeting(Guid meetingId, string message)
        {
            await _meetingsHub.Clients.All.SendAsync("MeetingReminder", new { meetingId, message });
        }

    }
}
