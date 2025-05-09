namespace Meetmind.Domain.Models.Realtime;

public record ConfirmActionMessage
{
    public Guid MeetingId { get; init; }
    public string Action { get; init; } = "";
    public bool Accepted { get; init; }
    public DateTime ConfirmedAtUtc { get; init; } = DateTime.UtcNow;
}