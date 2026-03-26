using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Commands;

public record UploadAccountStatementCommand(Guid DocumentId, Stream FileStream, string FileName) : IRequest<Result<Unit>>;

public class UploadAccountStatementCommandHandler : IRequestHandler<UploadAccountStatementCommand, Result<Unit>>
{
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IFileStorageService _fileStorage;
    private readonly INotificationService _notificationService;

    public UploadAccountStatementCommandHandler(
        IIssuedDocumentRepository documentRepo,
        IDocumentRequestRepository requestRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IFileStorageService fileStorage,
        INotificationService notificationService)
    {
        _documentRepo = documentRepo;
        _requestRepo = requestRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
        _fileStorage = fileStorage;
        _notificationService = notificationService;
    }

    public async Task<Result<Unit>> Handle(UploadAccountStatementCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Department != Department.Statistics)
            return Result.Failure<Unit>("Only Statistics department can upload account statements.", "FORBIDDEN");

        var doc = await _documentRepo.GetByIdWithDetailsAsync(request.DocumentId, cancellationToken);
        if (doc is null || doc.IsDeleted)
            return Result.Failure<Unit>("Document not found.", "NOT_FOUND");

        if (doc.Status == DocumentStatus.Archived || doc.Status == DocumentStatus.Revoked)
            return Result.Failure<Unit>("Account statement cannot be uploaded for archived or revoked documents.", "INVALID_STATUS");

        if (!string.IsNullOrEmpty(doc.AccountStatementPath))
            return Result.Failure<Unit>("Account statement has already been uploaded.", "ALREADY_UPLOADED");

        var fileName = $"{doc.Id}_account_statement.pdf";
        var path = await _fileStorage.SavePdfAsync(request.FileStream, fileName, cancellationToken);

        doc.AccountStatementPath = path;
        doc.UpdatedAt = DateTime.UtcNow;

        if (doc.MedicalReportUploadedAt.HasValue)
        {
            // Medical report already uploaded — archive now
            doc.Status = DocumentStatus.Archived;
            doc.ArchivedAt = DateTime.UtcNow;

            var req = await _requestRepo.GetByIdAsync(doc.RequestId, cancellationToken);
            if (req is not null)
            {
                req.Status = RequestStatus.Completed;
                req.UpdatedAt = DateTime.UtcNow;
                _requestRepo.Update(req);
            }
        }

        _documentRepo.Update(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("document.account_statement_uploaded", "document", doc.Id.ToString(),
            new { doc.DocumentNumber }, cancellationToken);

        if (doc.Status == DocumentStatus.Archived)
        {
            await _notificationService.SendToUserAsync(
                doc.IssuedById,
                "اكتملت الوثيقة",
                "Document Completed",
                $"تم رفع كشف الحساب وأرشفة الوثيقة رقم {doc.DocumentNumber}",
                $"Account statement uploaded and document #{doc.DocumentNumber} is now archived.",
                "document", doc.Id.ToString(), cancellationToken);
        }

        return Result.Success(Unit.Value);
    }
}
