
namespace Meetmind.Domain.Models;

public sealed class CalendarSyncLog
{
    public int Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string Source { get; set; } = default!;
    public int TotalEventsFound { get; set; }
    public int MeetingsCreated { get; set; }
    public string? ErrorMessage { get; set; }
}
