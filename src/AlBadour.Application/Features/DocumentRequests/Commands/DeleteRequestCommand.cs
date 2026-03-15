using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.DocumentRequests.Commands;

public record DeleteRequestCommand(Guid Id) : IRequest<Result>;

public class DeleteRequestCommandHandler : IRequestHandler<DeleteRequestCommand, Result>
{
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public DeleteRequestCommandHandler(
        IDocumentRequestRepository requestRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService)
    {
        _requestRepo = requestRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result> Handle(DeleteRequestCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Supervisor && _currentUser.Role != UserRole.Admin)
            return Result.Failure("Only supervisors and admins can delete requests.", "FORBIDDEN");

        var entity = await _requestRepo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null || entity.IsDeleted)
            return Result.Failure("Request not found.", "NOT_FOUND");

        if (entity.Status == RequestStatus.Completed || entity.Status == RequestStatus.Rejected)
            return Result.Failure("Completed or rejected requests cannot be deleted.", "INVALID_STATUS");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        _requestRepo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("request.deleted", "request", entity.Id.ToString(), null, cancellationToken);

        return Result.Success();
    }
}
