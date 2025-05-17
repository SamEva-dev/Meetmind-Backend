
namespace Meetmind.Application.Dto;

public sealed class MeetingDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public DateTime StartUtc { get; init; }
    public DateTime? EndUtc { get; init; }
    public string State { get; init; } = default!;

    public string TranscriptState { get; init; } = default!;
    public string? TranscriptPath { get; init; }

    public string SummaryState { get; init; } = default!;
    public string? SummaryPath { get; init; }

    public TimeSpan? Duration =>
        EndUtc.HasValue ? EndUtc.Value - StartUtc : null;
}
