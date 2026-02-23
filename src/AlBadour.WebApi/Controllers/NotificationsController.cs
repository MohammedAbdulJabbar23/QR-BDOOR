using AlBadour.Application.Features.Notifications.Commands;
using AlBadour.Application.Features.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlBadour.WebApi.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetMyNotificationsQuery(page, pageSize));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _mediator.Send(new GetUnreadCountQuery());
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { count = result.Value });
    }

    [HttpPut("{id:long}/read")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        var result = await _mediator.Send(new MarkAsReadCommand(id));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { message = "Notification marked as read." });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await _mediator.Send(new MarkAsReadCommand(null));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { message = "All notifications marked as read." });
    }
}
