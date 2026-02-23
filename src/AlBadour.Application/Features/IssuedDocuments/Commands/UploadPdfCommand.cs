using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Commands;

public record UploadPdfCommand(Guid DocumentId, Stream PdfStream, string FileName) : IRequest<Result>;

public class UploadPdfCommandHandler : IRequestHandler<UploadPdfCommand, Result>
{
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IFileStorageService _fileStorage;
    private readonly INotificationService _notificationService;

    public UploadPdfCommandHandler(
        IIssuedDocumentRepository documentRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IFileStorageService fileStorage,
        INotificationService notificationService)
    {
        _documentRepo = documentRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
        _fileStorage = fileStorage;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(UploadPdfCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Department != Department.Statistics)
            return Result.Failure("Only Statistics department staff can upload PDFs.", "FORBIDDEN");

        var document = await _documentRepo.GetByIdWithDetailsAsync(request.DocumentId, cancellationToken);
        if (document is null || document.IsDeleted)
            return Result.Failure("Document not found.", "NOT_FOUND");

        if (document.Status != DocumentStatus.Draft)
            return Result.Failure("PDF can only be uploaded for draft documents.", "INVALID_STATUS");

        var pdfPath = await _fileStorage.SavePdfAsync(request.PdfStream, $"{document.Id}.pdf", cancellationToken);

        document.PdfFilePath = pdfPath;
        document.Status = DocumentStatus.Archived;
        document.ArchivedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;

        _documentRepo.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("document.pdf_uploaded", "document", document.Id.ToString(),
            new { document.DocumentNumber, FileName = request.FileName }, cancellationToken);

        await _notificationService.SendToUserAsync(
            document.Request.CreatedById,
            "تم أرشفة وثيقتك",
            "Document Archived",
            $"تم رفع النسخة الموقعة وأرشفة الوثيقة رقم {document.DocumentNumber}",
            $"The signed copy has been uploaded and document #{document.DocumentNumber} is now archived.",
            "document", document.Id.ToString(), cancellationToken);

        return Result.Success();
    }
}
