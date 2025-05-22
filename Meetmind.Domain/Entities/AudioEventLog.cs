using Meetmind.Domain.Units;

namespace Meetmind.Domain.Entities;

public class AudioEventLog : AggregateRoot
{
    public Guid MeetingId { get; set; }
    public string Action { get; set; }     // "Start", "Pause", "Resume", "Stop", "Error"
    public DateTime UtcTimestamp { get; set; }
    public string? UserId { get; set; }
    public string? Details { get; set; }   // message/log/error
}
