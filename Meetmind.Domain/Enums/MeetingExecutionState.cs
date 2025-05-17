
namespace Meetmind.Domain.Enums;

public enum MeetingExecutionState
{
    Scheduled,
    WaitingUserConfirmation,
    Recording,
    Transcribing,
    Summarizing,
    Completed,
    Cancelled,
    Failed,
    Paused
}
