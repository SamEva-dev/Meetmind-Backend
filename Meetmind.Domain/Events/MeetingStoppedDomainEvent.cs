
using Meetmind.Domain.Events.Interface;

namespace Meetmind.Domain.Events;

public sealed class MeetingStoppedDomainEvent : IDomainEvent
{
    public Guid MeetingId { get; }

    public DateTime OccurredOn => DateTime.UtcNow;

    public MeetingStoppedDomainEvent(Guid meetingId) => MeetingId = meetingId;
}
