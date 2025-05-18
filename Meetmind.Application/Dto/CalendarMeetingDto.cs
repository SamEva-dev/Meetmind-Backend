namespace Meetmind.Application.Dto;

public sealed class CalendarMeetingDto
{
    public string ExternalId { get; init; } = default!;
    public string Source { get; init; } = default!;

    public string Title { get; init; } = default!;
    public DateTime Start { get; init; }
    public DateTime? End { get; init; }
    public string? OrganizerEmail { get; init; }
    public List<string>? AttendeesEmails { get; init; }
}
