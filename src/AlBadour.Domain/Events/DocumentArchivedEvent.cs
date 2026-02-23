namespace AlBadour.Domain.Events;

public sealed record DocumentArchivedEvent(Guid DocumentId, string DocumentNumber) : DomainEvent;
