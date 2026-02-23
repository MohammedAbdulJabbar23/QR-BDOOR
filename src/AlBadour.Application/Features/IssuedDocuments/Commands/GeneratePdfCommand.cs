using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Commands;

public record GeneratePdfCommand(Guid DocumentId) : IRequest<Result>;

public class GeneratePdfCommandHandler : IRequestHandler<GeneratePdfCommand, Result>
{
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IFileStorageService _fileStorage;
    private readonly IPdfGenerationService _pdfService;

    public GeneratePdfCommandHandler(
        IIssuedDocumentRepository documentRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IFileStorageService fileStorage,
        IPdfGenerationService pdfService)
    {
        _documentRepo = documentRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
        _fileStorage = fileStorage;
        _pdfService = pdfService;
    }

    public async Task<Result> Handle(GeneratePdfCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Department != Department.Statistics)
            return Result.Failure("Only Statistics department staff can generate PDFs.", "FORBIDDEN");

        var document = await _documentRepo.GetByIdWithDetailsAsync(request.DocumentId, cancellationToken);
        if (document is null || document.IsDeleted)
            return Result.Failure("Document not found.", "NOT_FOUND");

        if (document.Status != DocumentStatus.Draft)
            return Result.Failure("PDF can only be generated for draft documents.", "INVALID_STATUS");

        if (string.IsNullOrEmpty(document.QrCodeImagePath))
            return Result.Failure("QR code image must exist before generating PDF.", "MISSING_QR");

        // Fetch QR image bytes from MinIO
        var qrStream = await _fileStorage.GetFileAsync(document.QrCodeImagePath, cancellationToken);
        if (qrStream is null)
            return Result.Failure("QR code image not found in storage.", "MISSING_QR");

        byte[] qrImageBytes;
        using (var ms = new MemoryStream())
        {
            await qrStream.CopyToAsync(ms, cancellationToken);
            qrImageBytes = ms.ToArray();
        }
        if (qrStream is IDisposable disposable)
            disposable.Dispose();

        var pdfData = new PdfDocumentData(
            DocumentNumber: document.DocumentNumber,
            PatientName: document.Request.PatientName,
            PatientNameEn: document.Request.PatientNameEn,
            RecipientEntity: document.Request.RecipientEntity,
            DocumentTypeNameAr: document.Request.DocumentType.NameAr,
            DocumentTypeNameEn: document.Request.DocumentType.NameEn,
            DocumentBody: document.DocumentBody ?? string.Empty,
            QrCodeUrl: document.QrCodeUrl,
            QrCodeImageBytes: qrImageBytes,
            IssuedByName: document.IssuedBy.FullName,
            IssuedAt: document.IssuedAt
        );

        var pdfBytes = _pdfService.GenerateDocumentPdf(pdfData);

        using var pdfStream = new MemoryStream(pdfBytes);
        var pdfPath = await _fileStorage.SavePdfAsync(pdfStream, $"{document.Id}.pdf", cancellationToken);

        document.PdfFilePath = pdfPath;
        document.UpdatedAt = DateTime.UtcNow;

        _documentRepo.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("document.pdf_generated", "document", document.Id.ToString(),
            new { document.DocumentNumber }, cancellationToken);

        return Result.Success();
    }
}
