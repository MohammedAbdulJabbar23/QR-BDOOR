namespace AlBadour.Domain.Events;

public sealed record RequestRejectedEvent(Guid RequestId, Guid CreatedById, string Reason) : DomainEvent;
