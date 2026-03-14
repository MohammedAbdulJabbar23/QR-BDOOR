namespace AlBadour.Application.Features.IssuedDocuments.DTOs;

public record PrepareDocumentDto(
    Guid RequestId,
    string? DocumentBody,
    string? PatientGender,
    string? PatientProfession,
    string? PatientAge,
    string? AdmissionDate,
    string? DischargeDate,
    string? LeaveGranted
);
