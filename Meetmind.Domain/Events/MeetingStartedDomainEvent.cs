
using Meetmind.Domain.Events.Interface;

namespace Meetmind.Domain.Events;

public sealed class MeetingStartedDomainEvent : IDomainEvent
{
    public Guid MeetingId { get; }

    public DateTime OccurredOn => DateTime.UtcNow;

    public MeetingStartedDomainEvent(Guid meetingId) => MeetingId = meetingId;
}
