namespace AlBadour.Domain.Events;

public sealed record ModificationRequestedEvent(Guid DocumentId, Guid RequestedById) : DomainEvent;
