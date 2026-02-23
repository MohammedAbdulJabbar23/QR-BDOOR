using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.DocumentRequests.Commands;

public record AcceptRequestCommand(Guid Id) : IRequest<Result>;

public class AcceptRequestCommandHandler : IRequestHandler<AcceptRequestCommand, Result>
{
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public AcceptRequestCommandHandler(
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

    public async Task<Result> Handle(AcceptRequestCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Department != Department.Statistics)
            return Result.Failure("Only Statistics department staff can accept requests.", "FORBIDDEN");

        var entity = await _requestRepo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null || entity.IsDeleted)
            return Result.Failure("Request not found.", "NOT_FOUND");

        if (entity.Status != RequestStatus.Pending)
            return Result.Failure("Only pending requests can be accepted.", "INVALID_STATUS");

        entity.Status = RequestStatus.InProgress;
        entity.AssignedToId = _currentUser.UserId;
        entity.UpdatedAt = DateTime.UtcNow;

        _requestRepo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("request.accepted", "request", entity.Id.ToString(), null, cancellationToken);

        return Result.Success();
    }
}
