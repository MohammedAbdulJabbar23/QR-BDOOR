namespace AlBadour.Domain.Events;

public sealed record RequestCreatedEvent(Guid RequestId, Guid CreatedById) : DomainEvent;
