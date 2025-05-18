
using Meetmind.Domain.Events.Interface;

namespace Meetmind.Domain.Events;

public sealed class MeetingPausedDomainEvent : IDomainEvent
{
    public Guid MeetingId { get; }

    public DateTime OccurredOn => DateTime.UtcNow;

    public MeetingPausedDomainEvent(Guid meetingId) => MeetingId = meetingId;
}
