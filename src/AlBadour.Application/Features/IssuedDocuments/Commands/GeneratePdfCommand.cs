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
    private readonly IDocumentGenerationService _docService;

    public GeneratePdfCommandHandler(
        IIssuedDocumentRepository documentRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IFileStorageService fileStorage,
        IDocumentGenerationService docService)
    {
        _documentRepo = documentRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
        _fileStorage = fileStorage;
        _docService = docService;
    }

    public async Task<Result> Handle(GeneratePdfCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepo.GetByIdWithDetailsAsync(request.DocumentId, cancellationToken);
        if (document is null || document.IsDeleted)
            return Result.Failure("Document not found.", "NOT_FOUND");

        var isAdminLetter = document.Request.DocumentType.NameEn.Equals("Administrative Letter", StringComparison.OrdinalIgnoreCase);
        var allowedDept = isAdminLetter ? Department.HR : Department.Statistics;
        if (_currentUser.Department != allowedDept)
            return Result.Failure($"Only {allowedDept} department staff can generate this document.", "FORBIDDEN");

        if (document.Status != DocumentStatus.Draft)
            return Result.Failure("Document can only be generated for draft documents.", "INVALID_STATUS");

        if (string.IsNullOrEmpty(document.QrCodeImagePath))
            return Result.Failure("QR code image must exist before generating document.", "MISSING_QR");

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

        var docData = new DocumentGenerationData(
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
            IssuedAt: document.IssuedAt,
            PatientGender: document.PatientGender,
            PatientProfession: document.PatientProfession,
            PatientAge: document.PatientAge,
            AdmissionDate: document.AdmissionDate,
            DischargeDate: document.DischargeDate,
            LeaveGranted: document.LeaveGranted
        );

        var pdfBytes = _docService.GenerateDocument(docData);

        using var pdfStream = new MemoryStream(pdfBytes);
        var pdfPath = await _fileStorage.SavePdfAsync(pdfStream, $"{document.Id}.pdf", cancellationToken);

        document.PdfFilePath = pdfPath;
        document.UpdatedAt = DateTime.UtcNow;

        _documentRepo.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("document.generated", "document", document.Id.ToString(),
            new { document.DocumentNumber }, cancellationToken);

        return Result.Success();
    }
}
