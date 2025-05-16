namespace Meetmind.Domain.Units;

public class AggregateRoot
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

