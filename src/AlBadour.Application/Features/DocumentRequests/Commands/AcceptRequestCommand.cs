using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;
using System;

namespace AlBadour.Application.Features.DocumentRequests.Commands;

public record AcceptRequestCommand(Guid Id, Guid? DocumentTypeId = null) : IRequest<Result>;

public class AcceptRequestCommandHandler : IRequestHandler<AcceptRequestCommand, Result>
{
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly IDocumentTypeRepository _typeRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public AcceptRequestCommandHandler(
        IDocumentRequestRepository requestRepo,
        IDocumentTypeRepository typeRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _requestRepo = requestRepo;
        _typeRepo = typeRepo;
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
        var isMoiInsuranceLetter = entity.DocumentType.NameEn.Equals("MOI Insurance Letter", StringComparison.OrdinalIgnoreCase);

        if (_currentUser.Department == Department.MoiInsurance && !isMoiInsuranceLetter)
            return Result.Failure("MOI Insurance department can only accept MOI Insurance Letter requests.", "FORBIDDEN");

        if (_currentUser.Department != Department.MoiInsurance && isMoiInsuranceLetter)
            return Result.Failure("Only MOI Insurance department staff can accept this request.", "FORBIDDEN");

        if (_currentUser.Department == Department.HR && !isAdministrativeLetter)
            return Result.Failure("HR department can only accept Administrative Letter requests.", "FORBIDDEN");

        if (_currentUser.Department == Department.Statistics && (isAdministrativeLetter || isMoiInsuranceLetter))
            return Result.Failure("Statistics department cannot accept this request type.", "FORBIDDEN");

        if (_currentUser.Department != Department.HR && _currentUser.Department != Department.Statistics
            && _currentUser.Department != Department.MoiInsurance)
            return Result.Failure("Only HR, Statistics, and MOI Insurance department staff can accept this request.", "FORBIDDEN");

        if (entity.Status != RequestStatus.Pending)
            return Result.Failure("Only pending requests can be accepted.", "INVALID_STATUS");

        // Statistics can override the document type (e.g. switch between with/without table)
        if (request.DocumentTypeId.HasValue && _currentUser.Department == Department.Statistics)
        {
            var newDocType = await _typeRepo.GetByIdAsync(request.DocumentTypeId.Value, cancellationToken);
            if (newDocType is null || !newDocType.IsActive)
                return Result.Failure("Invalid or inactive document type.", "INVALID_DOCUMENT_TYPE");

            if (newDocType.NameEn.Equals("Administrative Letter", StringComparison.OrdinalIgnoreCase))
                return Result.Failure("Cannot change to Administrative Letter type.", "INVALID_DOCUMENT_TYPE");

            var originalHasAS = entity.DocumentType.NameEn.Contains("Account Statement", StringComparison.OrdinalIgnoreCase);
            var newHasAS = newDocType.NameEn.Contains("Account Statement", StringComparison.OrdinalIgnoreCase);
            if (originalHasAS != newHasAS)
                return Result.Failure("Cannot change the Account Statement status of the request.", "INVALID_DOCUMENT_TYPE");

            entity.DocumentTypeId = request.DocumentTypeId.Value;
            entity.DocumentType = newDocType;
        }

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

        // Notify Accounts department if this is an Account Statement type
        if (entity.DocumentType.NameEn.Contains("Account Statement", StringComparison.OrdinalIgnoreCase))
        {
            await _notificationService.SendToDepartmentAsync(
                Department.Accounts,
                "طلب جديد يتطلب كشف حساب",
                "New Account Statement Request",
                $"يوجد طلب جديد بانتظار رفع كشف الحساب",
                $"A new request requires an account statement upload.",
                "request", entity.Id.ToString(), cancellationToken);
        }

        return Result.Success();
    }
}
