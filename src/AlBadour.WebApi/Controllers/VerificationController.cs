using AlBadour.Application.Features.IssuedDocuments.Queries;
using AlBadour.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AlBadour.WebApi.Controllers;

[ApiController]
[Route("api/verify")]
public class VerificationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorage;
    private readonly IIssuedDocumentRepository _documentRepo;

    public VerificationController(IMediator mediator, IFileStorageService fileStorage, IIssuedDocumentRepository documentRepo)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
        _documentRepo = documentRepo;
    }

    [HttpGet("{documentId:guid}")]
    public async Task<IActionResult> Verify(Guid documentId)
    {
        var result = await _mediator.Send(new VerifyDocumentQuery(documentId));
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("{documentId:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid documentId)
    {
        var doc = await _documentRepo.GetByIdAsync(documentId);
        if (doc is null || doc.IsDeleted || string.IsNullOrEmpty(doc.PdfFilePath))
            return NotFound(new { error = "PDF not available." });

        // Only serve PDF for archived documents
        if (doc.Status != Domain.Enums.DocumentStatus.Archived)
            return NotFound(new { error = "PDF not available." });

        var stream = await _fileStorage.GetFileAsync(doc.PdfFilePath);
        if (stream is null) return NotFound(new { error = "PDF file not found." });

        return File(stream, "application/pdf", $"{doc.DocumentNumber}.pdf");
    }
}
