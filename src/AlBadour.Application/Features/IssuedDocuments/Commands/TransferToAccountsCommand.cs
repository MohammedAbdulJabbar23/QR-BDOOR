using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Commands;

public record TransferToAccountsCommand(Guid DocumentId) : IRequest<Result<Unit>>;

public class TransferToAccountsCommandHandler : IRequestHandler<TransferToAccountsCommand, Result<Unit>>
{
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public TransferToAccountsCommandHandler(
        IIssuedDocumentRepository documentRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _documentRepo = documentRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<Result<Unit>> Handle(TransferToAccountsCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Department != Department.Statistics)
            return Result.Failure<Unit>("Only Statistics department can transfer documents to Accounts.", "FORBIDDEN");

        var doc = await _documentRepo.GetByIdWithDetailsAsync(request.DocumentId, cancellationToken);
        if (doc is null || doc.IsDeleted)
            return Result.Failure<Unit>("Document not found.", "NOT_FOUND");

        if (doc.Status != DocumentStatus.Draft)
            return Result.Failure<Unit>("Document must be in Draft status to transfer.", "INVALID_STATUS");

        if (doc.MedicalReportUploadedAt is null)
            return Result.Failure<Unit>("The signed medical report must be uploaded before transferring to Accounts.", "PDF_REQUIRED");

        // Only Account Statement types can be transferred to Accounts
        var typeName = doc.Request.DocumentType.NameEn;
        if (!typeName.Contains("Account Statement", StringComparison.OrdinalIgnoreCase))
            return Result.Failure<Unit>("Only documents with Account Statement type can be transferred to Accounts.", "INVALID_TYPE");

        doc.Status = DocumentStatus.AwaitingAccountStatement;
        doc.UpdatedAt = DateTime.UtcNow;
        _documentRepo.Update(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("document.transferred_to_accounts", "document", doc.Id.ToString(),
            new { doc.DocumentNumber }, cancellationToken);

        await _notificationService.SendToDepartmentAsync(
            Department.Accounts,
            "وثيقة بحاجة لكشف حساب",
            "Document Needs Account Statement",
            $"الوثيقة رقم {doc.DocumentNumber} بحاجة لرفع كشف حساب",
            $"Document #{doc.DocumentNumber} needs an account statement uploaded.",
            "document", doc.Id.ToString(), cancellationToken);

        return Result.Success(Unit.Value);
    }
}
