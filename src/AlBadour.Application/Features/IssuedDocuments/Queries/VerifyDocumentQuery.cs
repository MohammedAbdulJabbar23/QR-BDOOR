using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.IssuedDocuments.DTOs;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Queries;

public record VerifyDocumentQuery(Guid DocumentId) : IRequest<Result<VerificationResultDto>>;

public class VerifyDocumentQueryHandler : IRequestHandler<VerifyDocumentQuery, Result<VerificationResultDto>>
{
    private readonly IIssuedDocumentRepository _documentRepo;

    public VerifyDocumentQueryHandler(IIssuedDocumentRepository documentRepo)
    {
        _documentRepo = documentRepo;
    }

    public async Task<Result<VerificationResultDto>> Handle(VerifyDocumentQuery request, CancellationToken cancellationToken)
    {
        var doc = await _documentRepo.GetByIdWithDetailsAsync(request.DocumentId, cancellationToken);

        // Document not found or draft or deleted = invalid
        if (doc is null || doc.IsDeleted || doc.Status == DocumentStatus.Draft)
        {
            return Result.Success(new VerificationResultDto(
                "invalid", null, null, null, null, null, null, null, null, false, false));
        }

        // Check QR expiry if set
        if (doc.QrExpiresAt.HasValue && doc.QrExpiresAt.Value < DateTime.UtcNow)
        {
            return Result.Success(new VerificationResultDto(
                "expired", doc.DocumentNumber, null, null, doc.IssuedAt, null, null, null, null, false, false));
        }

        var hasAccountStatement = !string.IsNullOrEmpty(doc.AccountStatementPath);

        if (doc.Status == DocumentStatus.Archived)
        {
            return Result.Success(new VerificationResultDto(
                "verified",
                doc.DocumentNumber,
                doc.Request.PatientName,
                doc.Request.RecipientEntity,
                doc.IssuedAt,
                null, null, null, null,
                !string.IsNullOrEmpty(doc.PdfFilePath),
                hasAccountStatement
            ));
        }

        if (doc.Status == DocumentStatus.Revoked)
        {
            return Result.Success(new VerificationResultDto(
                "revoked",
                doc.DocumentNumber,
                doc.Request.PatientName,
                doc.Request.RecipientEntity,
                doc.IssuedAt,
                doc.RevokedAt,
                doc.RevocationReason,
                doc.ReplacementDocumentId,
                doc.ReplacementDocument?.DocumentNumber,
                false,
                false
            ));
        }

        return Result.Success(new VerificationResultDto(
            "invalid", null, null, null, null, null, null, null, null, false, false));
    }
}
