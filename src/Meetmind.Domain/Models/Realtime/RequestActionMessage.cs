
namespace Meetmind.Domain.Models.Realtime;

public record RequestActionMessage
{
    public Guid MeetingId { get; init; }
    public string Action { get; init; } = ""; // e.g. "start_record", "transcribe", "summarize"
    public string Label { get; init; } = "";  // e.g. "Start recording now?"
    public DateTime TriggeredAtUtc { get; init; }
}