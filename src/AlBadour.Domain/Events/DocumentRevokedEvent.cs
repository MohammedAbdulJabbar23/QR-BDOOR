namespace AlBadour.Domain.Events;

public sealed record DocumentRevokedEvent(Guid DocumentId, string DocumentNumber, string Reason) : DomainEvent;
