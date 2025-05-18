using MediatR;
using Meetmind.Domain.Events.Interface;

namespace Meetmind.Domain.Events.Handlers;

public sealed class MeetingStartedHandler : INotificationHandler<MeetingStartedDomainEvent>
{
    private readonly INotificationService _notifier;

    public MeetingStartedHandler(INotificationService notifier)
    {
        _notifier = notifier;
    }

    public async Task Handle(MeetingStartedDomainEvent notification, CancellationToken ct)
    {
        await _notifier.NotifyMeetingStarted(notification.MeetingId);
    }
}
