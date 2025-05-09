using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meetmind.Domain.Models.Realtime;

public record UpcomingMeetingNotification
{
    public Guid MeetingId { get; init; }
    public string Title { get; init; } = "";
    public DateTime StartUtc { get; init; }
    public int MinutesBefore { get; init; }
    public string Source { get; init; } = "calendar";
}