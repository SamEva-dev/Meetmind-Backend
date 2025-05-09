using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meetmind.Domain.Models;

public record UpcomingMeeting
{
    public Guid Id { get; init; }            // interne si DB / random sinon
    public string Title { get; init; } = "";
    public DateTime StartUtc { get; init; }
    public DateTime EndUtc { get; init; }
    public string Source { get; init; } = ""; // "google" | "outlook" | "local"
    public List<string> Participants { get; init; } = [];
}