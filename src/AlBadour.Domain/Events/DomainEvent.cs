namespace AlBadour.Domain.Events;

public abstract record DomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
