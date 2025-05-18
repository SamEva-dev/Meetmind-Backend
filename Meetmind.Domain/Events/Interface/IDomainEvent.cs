
using MediatR;

namespace Meetmind.Domain.Events.Interface;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
