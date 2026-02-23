using AlBadour.Application.Features.AuditLogs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlBadour.WebApi.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? userId, [FromQuery] string? action,
        [FromQuery] string? entityType, [FromQuery] DateTime? from,
        [FromQuery] DateTime? to, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(userId, action, entityType, from, to, page, pageSize));
        if (!result.IsSuccess) return StatusCode(403, new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }
}
