namespace AlBadour.Domain.Exceptions;

public class UnauthorizedActionException : DomainException
{
    public UnauthorizedActionException(string message)
        : base("UNAUTHORIZED_ACTION", message) { }
}
