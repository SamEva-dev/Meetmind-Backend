using Meetmind.Domain.Units;

namespace Meetmind.Infrastructure.Events;

public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(IEnumerable<AggregateRoot> entitiesWithEvents);
}
