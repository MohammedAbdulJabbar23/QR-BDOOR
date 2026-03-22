namespace AlBadour.Application.Features.DocumentRequests.DTOs;

public record RequestDto(
    Guid Id,
    string PatientName,
    string? PatientNameEn,
    string RecipientEntity,
    Guid DocumentTypeId,
    string DocumentTypeNameAr,
    string DocumentTypeNameEn,
    string? Notes,
    string Status,
    string? RejectionReason,
    Guid CreatedById,
    string CreatedByName,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Language
);
