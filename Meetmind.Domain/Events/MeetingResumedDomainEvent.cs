using Meetmind.Domain.Events.Interface;

namespace Meetmind.Domain.Events;

public sealed class MeetingResumedDomainEvent : IDomainEvent
{
    public Guid MeetingId { get; }

    public DateTime OccurredOn => DateTime.UtcNow;

    public MeetingResumedDomainEvent(Guid meetingId) => MeetingId = meetingId;
}
