namespace AlBadour.Application.Features.Users.DTOs;

public record CreateUserDto(
    string Username,
    string FullName,
    string? FullNameEn,
    string Password,
    string Role,
    string Department
);
