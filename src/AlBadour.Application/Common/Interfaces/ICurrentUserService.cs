using AlBadour.Domain.Enums;

namespace AlBadour.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string UserName { get; }
    UserRole Role { get; }
    Department Department { get; }
    bool IsAuthenticated { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
