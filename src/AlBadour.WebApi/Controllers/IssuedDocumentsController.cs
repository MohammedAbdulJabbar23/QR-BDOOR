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

    public IssuedDocumentsController(IMediator mediator, IFileStorageService fileStorage)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
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
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        DocumentStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<DocumentStatus>(status, true, out var parsed))
            statusEnum = parsed;

        var result = await _mediator.Send(new GetAllDocumentsQuery(statusEnum, search, page, pageSize));
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

    [HttpPost("{id:guid}/upload-pdf")]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20MB limit
    public async Task<IActionResult> UploadPdf(Guid id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        using var stream = file.OpenReadStream();
        var result = await _mediator.Send(new UploadPdfCommand(id, stream, file.FileName));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { message = "PDF uploaded. Document is now archived and QR verification is active." });
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
