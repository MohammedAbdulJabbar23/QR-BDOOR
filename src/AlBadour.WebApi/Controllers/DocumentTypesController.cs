using AlBadour.Application.Features.DocumentTypes.Commands;
using AlBadour.Application.Features.DocumentTypes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlBadour.WebApi.Controllers;

[ApiController]
[Route("api/document-types")]
[Authorize]
public class DocumentTypesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentTypesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var result = await _mediator.Send(new GetDocumentTypesQuery(activeOnly));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDocTypeBody body)
    {
        var result = await _mediator.Send(new CreateDocumentTypeCommand(body.NameAr, body.NameEn, body.DescriptionAr, body.DescriptionEn));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { id = result.Value });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocTypeBody body)
    {
        var result = await _mediator.Send(new UpdateDocumentTypeCommand(id, body.NameAr, body.NameEn, body.DescriptionAr, body.DescriptionEn, body.IsActive));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { message = "Document type updated." });
    }
}

public record CreateDocTypeBody(string NameAr, string NameEn, string? DescriptionAr, string? DescriptionEn);
public record UpdateDocTypeBody(string NameAr, string NameEn, string? DescriptionAr, string? DescriptionEn, bool IsActive);
