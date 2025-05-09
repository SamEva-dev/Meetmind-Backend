using Meetmind.Domain.Enums;
using Meetmind.Domain.Models.Realtime;
using Meetmind.Domain.Models;
using Meetmind.Domain.ValueObjects;

namespace Meetmind.Application.Common.Interfaces;

public interface IMeetingOrchestratorManager
{
    Task StartOrchestratorAsync(UpcomingMeeting meeting, UserSettings settings, CancellationToken ct);
    Task HandleConfirmationAsync(ConfirmActionMessage message, CancellationToken ct);
    MeetingExecutionState? GetMeetingState(Guid meetingId);
}