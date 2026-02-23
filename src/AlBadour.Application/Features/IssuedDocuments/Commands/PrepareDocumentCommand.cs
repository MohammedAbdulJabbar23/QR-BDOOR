using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.IssuedDocuments.DTOs;
using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Commands;

public record PrepareDocumentCommand(PrepareDocumentDto Dto) : IRequest<Result<DocumentDto>>;

public class PrepareDocumentCommandHandler : IRequestHandler<PrepareDocumentCommand, Result<DocumentDto>>
{
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IDocumentNumberService _docNumberService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IFileStorageService _fileStorage;

    public PrepareDocumentCommandHandler(
        IDocumentRequestRepository requestRepo,
        IIssuedDocumentRepository documentRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IDocumentNumberService docNumberService,
        IQrCodeService qrCodeService,
        IFileStorageService fileStorage)
    {
        _requestRepo = requestRepo;
        _documentRepo = documentRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
        _docNumberService = docNumberService;
        _qrCodeService = qrCodeService;
        _fileStorage = fileStorage;
    }

    public async Task<Result<DocumentDto>> Handle(PrepareDocumentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Department != Department.Statistics)
            return Result.Failure<DocumentDto>("Only Statistics department staff can prepare documents.", "FORBIDDEN");

        var req = await _requestRepo.GetByIdWithDetailsAsync(request.Dto.RequestId, cancellationToken);
        if (req is null || req.IsDeleted)
            return Result.Failure<DocumentDto>("Request not found.", "NOT_FOUND");

        if (req.Status != RequestStatus.InProgress)
            return Result.Failure<DocumentDto>("Request must be in progress to prepare a document.", "INVALID_STATUS");

        var documentId = Guid.NewGuid();
        var documentNumber = await _docNumberService.GenerateNextAsync(cancellationToken);

        // Generate QR code
        var baseUrl = "https://albadour-hospital.com"; // This should come from configuration
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
            DocumentBody = request.Dto.DocumentBody,
            Status = DocumentStatus.Draft,
            IssuedById = _currentUser.UserId,
            IssuedAt = DateTime.UtcNow
        };

        await _documentRepo.AddAsync(document, cancellationToken);

        // Update request status to Completed
        req.Status = RequestStatus.Completed;
        req.UpdatedAt = DateTime.UtcNow;
        _requestRepo.Update(req);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("document.prepared", "document", documentId.ToString(),
            new { documentNumber, RequestId = req.Id, PatientName = req.PatientName }, cancellationToken);

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
            document.DocumentBody,
            document.Status.ToString(),
            null, null, null, null,
            document.IssuedById,
            _currentUser.UserName,
            null, null,
            document.IssuedAt,
            null, null
        );

        return Result.Success(dto);
    }
}
