using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.IssuedDocuments.DTOs;
using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace AlBadour.Application.Features.IssuedDocuments.Commands;

public record PrepareDocumentCommand(PrepareDocumentDto Dto) : IRequest<Result<DocumentDto>>;

public class PrepareDocumentCommandHandler : IRequestHandler<PrepareDocumentCommand, Result<DocumentDto>>
{
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IFileStorageService _fileStorage;
    private readonly IConfiguration _configuration;
    private readonly INotificationService _notificationService;

    public PrepareDocumentCommandHandler(
        IDocumentRequestRepository requestRepo,
        IIssuedDocumentRepository documentRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IQrCodeService qrCodeService,
        IFileStorageService fileStorage,
        IConfiguration configuration,
        INotificationService notificationService)
    {
        _requestRepo = requestRepo;
        _documentRepo = documentRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
        _qrCodeService = qrCodeService;
        _fileStorage = fileStorage;
        _configuration = configuration;
        _notificationService = notificationService;
    }

    public async Task<Result<DocumentDto>> Handle(PrepareDocumentCommand request, CancellationToken cancellationToken)
    {
        var req = await _requestRepo.GetByIdWithDetailsAsync(request.Dto.RequestId, cancellationToken);
        if (req is null || req.IsDeleted)
            return Result.Failure<DocumentDto>("Request not found.", "NOT_FOUND");

        var isAdminLetter = req.DocumentType.NameEn.Equals("Administrative Letter", StringComparison.OrdinalIgnoreCase);
        var allowedDept = isAdminLetter ? Department.HR : Department.Statistics;
        if (_currentUser.Department != allowedDept)
            return Result.Failure<DocumentDto>($"Only {allowedDept} department staff can prepare this document.", "FORBIDDEN");

        if (req.Status != RequestStatus.InProgress)
            return Result.Failure<DocumentDto>("Request must be in progress to prepare a document.", "INVALID_STATUS");

        var documentNumber = request.Dto.DocumentNumber.Trim();
        if (string.IsNullOrWhiteSpace(documentNumber))
            return Result.Failure<DocumentDto>("Document number is required.", "VALIDATION_ERROR");

        if (await _documentRepo.ExistsByDocumentNumberAsync(documentNumber, cancellationToken))
            return Result.Failure<DocumentDto>("Document number already exists.", "DUPLICATE_DOCUMENT_NUMBER");

        var documentId = Guid.NewGuid();

        // Generate QR code
        var baseUrl = _configuration["BaseUrl"]?.TrimEnd('/') ?? "https://albadour-hospital.com";
        var qrUrl = $"{baseUrl}/verify/{documentId}";
        var qrImageBytes = _qrCodeService.GenerateQrCode(qrUrl);
        var qrImagePath = await _fileStorage.SaveQrCodeAsync(qrImageBytes, $"{documentId}.png", cancellationToken);

        var document = new IssuedDocument
        {
            Id = documentId,
            DocumentNumber = documentNumber,
            RequestId = req.Id,
            QrCodeUrl = qrUrl,
            QrCodeImagePath = qrImagePath,
            Subject = isAdminLetter ? request.Dto.Subject?.Trim() : null,
            DocumentBody = request.Dto.DocumentBody,
            PatientGender = isAdminLetter ? null : request.Dto.PatientGender,
            PatientProfession = isAdminLetter ? null : request.Dto.PatientProfession,
            PatientAge = isAdminLetter ? null : request.Dto.PatientAge,
            AdmissionDate = isAdminLetter ? null : request.Dto.AdmissionDate,
            DischargeDate = isAdminLetter ? null : request.Dto.DischargeDate,
            LeaveGranted = isAdminLetter ? null : request.Dto.LeaveGranted,
            Status = DocumentStatus.Draft,
            IssuedById = _currentUser.UserId,
            IssuedAt = DateTime.UtcNow
        };

        await _documentRepo.AddAsync(document, cancellationToken);

        req.UpdatedAt = DateTime.UtcNow;
        _requestRepo.Update(req);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("document.prepared", "document", documentId.ToString(),
            new { documentNumber, RequestId = req.Id, PatientName = req.PatientName }, cancellationToken);

        await _notificationService.SendToUserAsync(
            req.CreatedById,
            "تم إعداد وثيقتك",
            "Document Prepared",
            $"تم إعداد الوثيقة رقم {documentNumber} لطلبك",
            $"Document #{documentNumber} has been prepared for your request.",
            "document", documentId.ToString(), cancellationToken);

        var dto = new DocumentDto(
            document.Id,
            document.DocumentNumber,
            document.RequestId,
            req.PatientName,
            req.PatientNameEn,
            req.RecipientEntity,
            req.DocumentType.NameAr,
            req.DocumentType.NameEn,
            document.QrCodeUrl,
            document.QrCodeImagePath,
            false,
            document.Subject,
            document.DocumentBody,
            document.Status.ToString(),
            null, null, null, null,
            document.PatientGender,
            document.PatientProfession,
            document.PatientAge,
            document.AdmissionDate,
            document.DischargeDate,
            document.LeaveGranted,
            document.IssuedById,
            _currentUser.UserName,
            null, null,
            document.IssuedAt,
            null, null,
            false
        );

        return Result.Success(dto);
    }
}
