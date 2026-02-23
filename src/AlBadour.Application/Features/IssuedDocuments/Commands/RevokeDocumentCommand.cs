using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Commands;

public record RevokeDocumentCommand(Guid DocumentId, string Reason) : IRequest<Result>;

public class RevokeDocumentCommandHandler : IRequestHandler<RevokeDocumentCommand, Result>
{
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public RevokeDocumentCommandHandler(
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

    public async Task<Result> Handle(RevokeDocumentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Supervisor && _currentUser.Role != UserRole.Admin)
            return Result.Failure("Only supervisors and admins can revoke documents.", "FORBIDDEN");

        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure("Revocation reason is required.", "VALIDATION_ERROR");

        var document = await _documentRepo.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null || document.IsDeleted)
            return Result.Failure("Document not found.", "NOT_FOUND");

        if (document.Status != DocumentStatus.Archived)
            return Result.Failure("Only archived documents can be revoked.", "INVALID_STATUS");

        var beforeStatus = document.Status.ToString();

        document.Status = DocumentStatus.Revoked;
        document.RevocationReason = request.Reason;
        document.RevokedById = _currentUser.UserId;
        document.RevokedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;

        _documentRepo.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("document.revoked", "document", document.Id.ToString(),
            new { document.DocumentNumber, Reason = request.Reason, before = new { Status = beforeStatus }, after = new { Status = "Revoked" } },
            cancellationToken);

        await _notificationService.SendToRoleAsync(
            UserRole.Admin,
            $"تم إلغاء الوثيقة رقم {document.DocumentNumber}",
            $"Document {document.DocumentNumber} has been revoked",
            $"تم إلغاء الوثيقة رقم {document.DocumentNumber}. السبب: {request.Reason}",
            $"Document {document.DocumentNumber} has been revoked. Reason: {request.Reason}",
            "document", document.Id.ToString(), cancellationToken);

        return Result.Success();
    }
}
