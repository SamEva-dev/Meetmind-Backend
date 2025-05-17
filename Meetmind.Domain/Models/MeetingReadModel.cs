
namespace Meetmind.Domain.Models;

public sealed class MeetingReadModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public DateTime StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public string State { get; set; } = default!;
    public string? TranscriptPath { get; set; }
    public string? SummaryPath { get; set; }
    public string? ExternalId { get; set; }
    public string? ExternalSource { get; set; }
}
