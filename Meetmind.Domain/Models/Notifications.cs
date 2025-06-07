

namespace Meetmind.Domain.Models;

public class Notifications
{
    public Guid MeetingId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public DateTime Time { get; set; }
}
