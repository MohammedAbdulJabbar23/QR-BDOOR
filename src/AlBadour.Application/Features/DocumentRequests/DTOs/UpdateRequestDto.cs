namespace AlBadour.Application.Features.DocumentRequests.DTOs;

public record UpdateRequestDto(
    string? PatientName,
    string? PatientNameEn,
    string RecipientEntity,
    Guid DocumentTypeId,
    string? Notes
);
