namespace AlBadour.Application.Common.Interfaces;

public record DocumentGenerationData(
    string DocumentNumber,
    string PatientName,
    string? PatientNameEn,
    string RecipientEntity,
    string Subject,
    string DocumentTypeNameAr,
    string DocumentTypeNameEn,
    string DocumentBody,
    string QrCodeUrl,
    byte[] QrCodeImageBytes,
    string IssuedByName,
    DateTime IssuedAt,
    string? PatientGender,
    string? PatientProfession,
    string? PatientAge,
    string? AdmissionDate,
    string? DischargeDate,
    string? LeaveGranted
);

public interface IDocumentGenerationService
{
    byte[] GenerateDocument(DocumentGenerationData data);
}
