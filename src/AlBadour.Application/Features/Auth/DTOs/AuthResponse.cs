namespace AlBadour.Application.Features.Auth.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string Username,
    string FullName,
    string? FullNameEn,
    string Role,
    string Department,
    string LanguagePreference
);
