
namespace Meetmind.Application.Search;

public record SearchEntry
{
    public Guid MeetingId { get; init; }
    public string MatchType { get; init; } = "";
    public string Snippet { get; init; } = "";
    public double Score { get; init; } = 1.0;
    public DateTime DateUtc { get; init; }
}