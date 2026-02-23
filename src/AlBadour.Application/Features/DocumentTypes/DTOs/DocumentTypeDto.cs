namespace AlBadour.Application.Features.DocumentTypes.DTOs;

public record DocumentTypeDto(
    Guid Id,
    string NameAr,
    string NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    bool IsActive,
    DateTime CreatedAt
);
