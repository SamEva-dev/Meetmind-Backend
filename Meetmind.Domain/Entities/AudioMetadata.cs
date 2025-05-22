using Meetmind.Domain.Units;

namespace Meetmind.Domain.Entities;

public class AudioMetadata : AggregateRoot
{
    public Guid MeetingId { get; set; }
    public string FilePath { get; set; }
    public string Title { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public int FragmentCount { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? UserId { get; set; }
    public string? DeviceId { get; set; }
    public string? AppVersion { get; set; }
    public string? Extra { get; set; }
    public DateTime UploadedUtc { get; set; }
}
