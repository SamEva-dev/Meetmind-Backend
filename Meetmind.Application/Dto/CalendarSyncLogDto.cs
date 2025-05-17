using System;


namespace Meetmind.Application.Dto;

public sealed class CalendarSyncLogDto
{
    public DateTime TimestampUtc { get; init; }
    public string Source { get; init; } = default!;
    public int TotalEventsFound { get; init; }
    public int MeetingsCreated { get; init; }
    public string? ErrorMessage { get; init; }
}