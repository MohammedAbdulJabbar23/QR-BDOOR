namespace AlBadour.Application.Features.Users.DTOs;

public record UserDto(
    Guid Id,
    string Username,
    string FullName,
    string? FullNameEn,
    string Role,
    string Department,
    string LanguagePreference,
    bool IsActive,
    DateTime CreatedAt
);
