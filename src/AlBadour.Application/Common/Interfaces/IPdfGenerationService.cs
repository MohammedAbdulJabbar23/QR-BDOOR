namespace AlBadour.Application.Common.Interfaces;

public record PdfDocumentData(
    string DocumentNumber,
    string PatientName,
    string? PatientNameEn,
    string RecipientEntity,
    string DocumentTypeNameAr,
    string DocumentTypeNameEn,
    string DocumentBody,
    string QrCodeUrl,
    byte[] QrCodeImageBytes,
    string IssuedByName,
    DateTime IssuedAt
);

public interface IPdfGenerationService
{
    byte[] GenerateDocumentPdf(PdfDocumentData data);
}
