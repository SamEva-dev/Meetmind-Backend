using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meetmind.Application.Dto;

public sealed class CalendarMeetingDto
{
    public string ExternalId { get; init; } = default!; // ID unique venant de Google/Outlook
    public string Source { get; init; } = default!;     // "Google" ou "Outlook"

    public string Title { get; init; } = default!;
    public DateTime StartUtc { get; init; }
    public DateTime? EndUtc { get; init; }
    public string? OrganizerEmail { get; init; }
    public List<string>? AttendeesEmails { get; init; }
}
