namespace AlBadour.Application.Features.DocumentRequests.DTOs;

public record CreateRequestDto(
    string? PatientName,
    string? PatientNameEn,
    string RecipientEntity,
    Guid DocumentTypeId,
    string? Notes,
    string? Language
);
