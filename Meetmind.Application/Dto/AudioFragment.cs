
namespace Meetmind.Application.Dto;

public class AudioFragment
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public int SequenceNumber { get; set; } // pour garder l'ordre
    public string FilePath { get; set; }
    public DateTime UploadedUtc { get; set; }
    public string? UserId { get; set; }
}
