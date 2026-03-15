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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IFileStorageService _fileStorage;
    private readonly INotificationService _notificationService;

    public UploadAccountStatementCommandHandler(
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

    public async Task<Result<Unit>> Handle(UploadAccountStatementCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Department != Department.Accounts)
            return Result.Failure<Unit>("Only Accounts department can upload account statements.", "FORBIDDEN");

        var doc = await _documentRepo.GetByIdWithDetailsAsync(request.DocumentId, cancellationToken);
        if (doc is null || doc.IsDeleted)
            return Result.Failure<Unit>("Document not found.", "NOT_FOUND");

        if (doc.Status != DocumentStatus.AwaitingAccountStatement)
            return Result.Failure<Unit>("Document must be awaiting account statement.", "INVALID_STATUS");

        var fileName = $"{doc.Id}_account_statement.pdf";
        var path = await _fileStorage.SavePdfAsync(request.FileStream, fileName, cancellationToken);

        doc.AccountStatementPath = path;
        doc.Status = DocumentStatus.Draft;
        doc.UpdatedAt = DateTime.UtcNow;
        _documentRepo.Update(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("document.account_statement_uploaded", "document", doc.Id.ToString(),
            new { doc.DocumentNumber }, cancellationToken);

        await _notificationService.SendToUserAsync(
            doc.IssuedById,
            "تم رفع كشف الحساب",
            "Account Statement Uploaded",
            $"تم رفع كشف الحساب للوثيقة رقم {doc.DocumentNumber}",
            $"Account statement uploaded for document #{doc.DocumentNumber}.",
            "document", doc.Id.ToString(), cancellationToken);

        return Result.Success(Unit.Value);
    }
}
