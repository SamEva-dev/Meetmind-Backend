using Meetmind.Domain.Units;

namespace Meetmind.Domain.Entities;

public sealed class CalendarSyncLog : AggregateRoot
{
    public DateTime TimestampUtc { get; set; }
    public string Source { get; set; } = default!;
    public int TotalEventsFound { get; set; }
    public int MeetingsCreated { get; set; }
    public string? ErrorMessage { get; set; }
}
