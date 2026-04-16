using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.DocumentRequests.Commands;

public record RejectRequestCommand(Guid Id, string Reason) : IRequest<Result>;

public class RejectRequestCommandHandler : IRequestHandler<RejectRequestCommand, Result>
{
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public RejectRequestCommandHandler(
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

    public async Task<Result> Handle(RejectRequestCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure("Rejection reason is required.", "VALIDATION_ERROR");

        var entity = await _requestRepo.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (entity is null || entity.IsDeleted)
            return Result.Failure("Request not found.", "NOT_FOUND");

        var isAdministrativeLetter = entity.DocumentType.NameEn.Equals("Administrative Letter", StringComparison.OrdinalIgnoreCase);
        var isMoiInsuranceLetter = entity.DocumentType.NameEn.Equals("MOI Insurance Letter", StringComparison.OrdinalIgnoreCase);

        if (_currentUser.Department == Department.MoiInsurance && !isMoiInsuranceLetter)
            return Result.Failure("MOI Insurance department can only reject MOI Insurance Letter requests.", "FORBIDDEN");

        if (_currentUser.Department != Department.MoiInsurance && isMoiInsuranceLetter)
            return Result.Failure("Only MOI Insurance department staff can reject this request.", "FORBIDDEN");

        if (_currentUser.Department == Department.HR && !isAdministrativeLetter)
            return Result.Failure("HR department can only reject Administrative Letter requests.", "FORBIDDEN");

        if (_currentUser.Department == Department.Statistics && (isAdministrativeLetter || isMoiInsuranceLetter))
            return Result.Failure("Statistics department cannot reject this request type.", "FORBIDDEN");

        if (_currentUser.Department != Department.HR && _currentUser.Department != Department.Statistics
            && _currentUser.Department != Department.MoiInsurance)
            return Result.Failure("Only HR, Statistics, and MOI Insurance department staff can reject this request.", "FORBIDDEN");

        if (entity.Status != RequestStatus.Pending)
            return Result.Failure("Only pending requests can be rejected.", "INVALID_STATUS");

        entity.Status = RequestStatus.Rejected;
        entity.RejectionReason = request.Reason;
        entity.UpdatedAt = DateTime.UtcNow;

        _requestRepo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("request.rejected", "request", entity.Id.ToString(),
            new { Reason = request.Reason }, cancellationToken);

        await _notificationService.SendToUserAsync(
            entity.CreatedById,
            "تم رفض طلبك",
            "Request Rejected",
            $"تم رفض طلبك. السبب: {request.Reason}",
            $"Your request was rejected. Reason: {request.Reason}",
            "request", entity.Id.ToString(), cancellationToken);

        return Result.Success();
    }
}
