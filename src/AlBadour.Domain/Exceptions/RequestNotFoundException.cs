namespace AlBadour.Domain.Exceptions;

public class RequestNotFoundException : DomainException
{
    public RequestNotFoundException(Guid id)
        : base("REQUEST_NOT_FOUND", $"Request with ID '{id}' was not found.") { }
}
