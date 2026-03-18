using AlBadour.Application.Features.IssuedDocuments.Commands;
using AlBadour.Application.Features.IssuedDocuments.DTOs;
using AlBadour.Application.Features.IssuedDocuments.Queries;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlBadour.WebApi.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class IssuedDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorage;
    private readonly IIssuedDocumentRepository _documentRepo;

    public IssuedDocumentsController(IMediator mediator, IFileStorageService fileStorage, IIssuedDocumentRepository documentRepo)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
        _documentRepo = documentRepo;
    }

    [HttpPost]
    public async Task<IActionResult> Prepare([FromBody] PrepareDocumentDto dto)
    {
        var result = await _mediator.Send(new PrepareDocumentCommand(dto));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status, [FromQuery] string? search,
        [FromQuery] Guid? documentTypeId,
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;

        DocumentStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<DocumentStatus>(status, true, out var parsed))
            statusEnum = parsed;

        var result = await _mediator.Send(new GetAllDocumentsQuery(statusEnum, search, documentTypeId, fromDate, toDate, page, pageSize));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDocumentByIdQuery(id));
        if (!result.IsSuccess) return NotFound(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpGet("by-request/{requestId:guid}")]
    public async Task<IActionResult> GetByRequest(Guid requestId)
    {
        var result = await _mediator.Send(new GetDocumentsByRequestQuery(requestId));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/generate-pdf")]
    public async Task<IActionResult> GeneratePdf(Guid id)
    {
        var result = await _mediator.Send(new GeneratePdfCommand(id));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { message = "PDF generated. Print, sign, then upload the signed copy." });
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid id)
    {
        var docResult = await _mediator.Send(new GetDocumentByIdQuery(id));
        if (!docResult.IsSuccess || docResult.Value is not { HasPdf: true })
            return NotFound();

        var stream = await _fileStorage.GetFileAsync($"pdfs/{id}.pdf");
        if (stream is null) return NotFound();
        return File(stream, "application/pdf", $"{docResult.Value.DocumentNumber}.pdf");
    }

    [HttpPost("{id:guid}/upload-pdf")]
    public async Task<IActionResult> UploadPdf(Guid id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided.", code = "INVALID_FILE" });

        const long maxFileSize = 10 * 1024 * 1024; // 10 MB
        if (file.Length > maxFileSize)
            return BadRequest(new { error = "File size exceeds 10 MB limit.", code = "FILE_TOO_LARGE" });

        if (file.ContentType != "application/pdf" && !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only PDF files are allowed.", code = "INVALID_FILE_TYPE" });

        using var stream = file.OpenReadStream();
        var result = await _mediator.Send(new UploadPdfCommand(id, stream, file.FileName));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { message = "PDF uploaded. Document is now archived." });
    }

    [HttpPost("{id:guid}/transfer-to-accounts")]
    public async Task<IActionResult> TransferToAccounts(Guid id)
    {
        var result = await _mediator.Send(new TransferToAccountsCommand(id));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { message = "Document transferred to Accounts department." });
    }

    [HttpPost("{id:guid}/upload-account-statement")]
    public async Task<IActionResult> UploadAccountStatement(Guid id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided.", code = "INVALID_FILE" });

        const long maxFileSize = 10 * 1024 * 1024; // 10 MB
        if (file.Length > maxFileSize)
            return BadRequest(new { error = "File size exceeds 10 MB limit.", code = "FILE_TOO_LARGE" });

        if (file.ContentType != "application/pdf" && !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only PDF files are allowed.", code = "INVALID_FILE_TYPE" });

        using var stream = file.OpenReadStream();
        var result = await _mediator.Send(new UploadAccountStatementCommand(id, stream, file.FileName));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { message = "Account statement uploaded." });
    }

    [HttpGet("{id:guid}/account-statement")]
    public async Task<IActionResult> GetAccountStatement(Guid id)
    {
        var docResult = await _mediator.Send(new GetDocumentByIdQuery(id));
        if (!docResult.IsSuccess || docResult.Value is not { HasAccountStatement: true })
            return NotFound();

        var doc = await _documentRepo.GetByIdAsync(id);
        if (doc is null || doc.IsDeleted || string.IsNullOrEmpty(doc.AccountStatementPath))
            return NotFound();

        var stream = await _fileStorage.GetFileAsync(doc.AccountStatementPath);
        if (stream is null) return NotFound();
        return File(stream, "application/pdf", $"{doc.DocumentNumber}_account_statement.pdf");
    }

    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id, [FromBody] RevokeDocumentBody body)
    {
        var result = await _mediator.Send(new RevokeDocumentCommand(id, body.Reason));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { message = "Document revoked." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteDocumentCommand(id));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { message = "Document deleted." });
    }

    [HttpGet("{id:guid}/qr-image")]
    public async Task<IActionResult> GetQrImage(Guid id)
    {
        var docResult = await _mediator.Send(new GetDocumentByIdQuery(id));
        if (!docResult.IsSuccess || docResult.Value?.QrCodeImagePath is null)
            return NotFound();

        var stream = await _fileStorage.GetFileAsync(docResult.Value.QrCodeImagePath);
        if (stream is null) return NotFound();
        return File(stream, "image/png");
    }
}

public record RevokeDocumentBody(string Reason);
