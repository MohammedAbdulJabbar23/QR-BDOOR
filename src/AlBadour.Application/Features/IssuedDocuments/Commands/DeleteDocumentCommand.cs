using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Commands;

public record DeleteDocumentCommand(Guid DocumentId) : IRequest<Result>;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Result>
{
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public DeleteDocumentCommandHandler(
        IIssuedDocumentRepository documentRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService)
    {
        _documentRepo = documentRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Supervisor && _currentUser.Role != UserRole.Admin)
            return Result.Failure("Only supervisors and admins can delete documents.", "FORBIDDEN");

        var document = await _documentRepo.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null || document.IsDeleted)
            return Result.Failure("Document not found.", "NOT_FOUND");

        document.IsDeleted = true;
        document.UpdatedAt = DateTime.UtcNow;

        _documentRepo.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("document.deleted", "document", document.Id.ToString(),
            new { document.DocumentNumber }, cancellationToken);

        return Result.Success();
    }
}
