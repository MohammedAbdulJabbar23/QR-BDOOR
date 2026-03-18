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
    private readonly INotificationService _notificationService;

    public AcceptRequestCommandHandler(
        IDocumentRequestRepository requestRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _requestRepo = requestRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(AcceptRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _requestRepo.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (entity is null || entity.IsDeleted)
            return Result.Failure("Request not found.", "NOT_FOUND");

        var isAdministrativeLetter = entity.DocumentType.NameEn.Equals("Administrative Letter", StringComparison.OrdinalIgnoreCase);

        if (_currentUser.Department == Department.HR && !isAdministrativeLetter)
            return Result.Failure("HR department can only accept Administrative Letter requests.", "FORBIDDEN");

        if (_currentUser.Department == Department.Statistics && isAdministrativeLetter)
            return Result.Failure("Statistics department cannot accept Administrative Letter requests.", "FORBIDDEN");

        if (_currentUser.Department != Department.HR && _currentUser.Department != Department.Statistics)
            return Result.Failure("Only HR and Statistics department staff can accept this request.", "FORBIDDEN");

        if (entity.Status != RequestStatus.Pending)
            return Result.Failure("Only pending requests can be accepted.", "INVALID_STATUS");

        entity.Status = RequestStatus.InProgress;
        entity.AssignedToId = _currentUser.UserId;
        entity.UpdatedAt = DateTime.UtcNow;

        _requestRepo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("request.accepted", "request", entity.Id.ToString(), null, cancellationToken);

        await _notificationService.SendToUserAsync(
            entity.CreatedById,
            "تم قبول طلبك",
            "Request Accepted",
            "تم قبول طلبك وهو قيد المعالجة الآن",
            "Your request has been accepted and is now being processed.",
            "request", entity.Id.ToString(), cancellationToken);

        return Result.Success();
    }
}
