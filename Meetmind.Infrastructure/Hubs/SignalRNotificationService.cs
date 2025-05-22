using AutoMapper;
using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Events.Interface;
using Meetmind.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Meetmind.Infrastructure.Hubs
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<SettingsHub> _settingsHub;
        private readonly IHubContext<MeetingHub> _meetingsHub;
        private readonly IHubContext<AudioHub> _audioHub;
        private readonly IMapper _mapper;
        public SignalRNotificationService(
        IHubContext<SettingsHub> settingsHub, IMapper mapper)
        {
            _settingsHub = settingsHub;
            _mapper = mapper;
        }

        public async Task NotifyFragmentUploaded(Guid meetingId, int sequenceNumber)
        {
            await _meetingsHub.Clients.All.SendAsync("SteamAudio", new
            {
                MeetingId = meetingId,
                SequenceNumber = sequenceNumber
            });
        }

        public async Task NotifyMeetingPaused(Guid meetingId)
        {
            await _meetingsHub.Clients.All.SendAsync("MeetingPaused", new
            {
                MeetingId = meetingId,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyMeetingResumed(Guid meetingId)
        {
            await _meetingsHub.Clients.All.SendAsync("MeetingResumed", new
            {
                MeetingId = meetingId,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyMeetingStarted(Guid meetingId)
        {
            await _meetingsHub.Clients.All.SendAsync("MeetingStarted", new
            {
                MeetingId = meetingId,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyMeetingStopped(Guid meetingId)
        {
            await _meetingsHub.Clients.All.SendAsync("MeetingStopped", new
            {
                MeetingId = meetingId,
                Timestamp = DateTime.UtcNow
            });
        }

        public Task NotifyRecordingError(Guid meetingId, string error) =>
         _audioHub.Clients.Group(meetingId.ToString()).SendAsync("RecordingError", meetingId,  error);

        public async Task NotifySettingsUpdatedAsync(SettingsEntity settings)
        {
            await _settingsHub.Clients.All.SendAsync("SettingsUpdated", _mapper.Map<SettingsDto> (settings));
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
