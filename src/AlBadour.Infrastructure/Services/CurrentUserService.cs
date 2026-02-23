using System.Security.Claims;
using AlBadour.Application.Common.Interfaces;
using AlBadour.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace AlBadour.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var id = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return id is not null ? Guid.Parse(id) : Guid.Empty;
        }
    }

    public string UserName =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public UserRole Role
    {
        get
        {
            var role = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            return role is not null ? Enum.Parse<UserRole>(role, true) : UserRole.Employee;
        }
    }

    public Department Department
    {
        get
        {
            var dept = _httpContextAccessor.HttpContext?.User.FindFirstValue("department");
            return dept is not null ? Enum.Parse<Department>(dept, true) : Department.Inquiry;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();
}
