using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meetmind.Domain.Models.Realtime;

public record ActionResultMessage
{
    public Guid MeetingId { get; init; }
    public string Action { get; init; } = "";
    public string Status { get; init; } = "success"; // or "failed"
    public string? Message { get; init; }
    public DateTime CompletedAtUtc { get; init; } = DateTime.UtcNow;
}