namespace AlBadour.Application.Features.IssuedDocuments.DTOs;

public record VerificationResultDto(
    string Status,
    string? DocumentNumber,
    string? PatientName,
    string? RecipientEntity,
    DateTime? IssuedAt,
    DateTime? RevokedAt,
    string? RevocationReason,
    Guid? ReplacementDocumentId,
    string? ReplacementDocumentNumber,
    bool HasPdf,
    bool HasAccountStatement
);
