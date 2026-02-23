namespace AlBadour.Domain.Exceptions;

public class DocumentNotFoundException : DomainException
{
    public DocumentNotFoundException(Guid id)
        : base("DOCUMENT_NOT_FOUND", $"Document with ID '{id}' was not found.") { }
}
