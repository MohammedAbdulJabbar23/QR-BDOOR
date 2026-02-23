using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Features.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlBadour.WebApi.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IReportGenerationService _reportService;

    public ReportsController(IMediator mediator, IReportGenerationService reportService)
    {
        _mediator = mediator;
        _reportService = reportService;
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily([FromQuery] DateTime date)
    {
        var result = await _mediator.Send(new GetDailyReportQuery(date));
        if (!result.IsSuccess) return StatusCode(403, new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpGet("status-breakdown")]
    public async Task<IActionResult> GetStatusBreakdown([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _mediator.Send(new GetStatusBreakdownQuery(from, to));
        if (!result.IsSuccess) return StatusCode(403, new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpGet("cancelled")]
    public async Task<IActionResult> GetCancelled([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _mediator.Send(new GetCancelledReportQuery(from, to));
        if (!result.IsSuccess) return StatusCode(403, new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpGet("export/{reportType}")]
    public async Task<IActionResult> Export(string reportType, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        byte[] fileBytes;
        string fileName;

        switch (reportType.ToLower())
        {
            case "daily":
                var dailyResult = await _mediator.Send(new GetDailyReportQuery(from));
                if (!dailyResult.IsSuccess) return BadRequest(new { error = dailyResult.Error });
                fileBytes = _reportService.GenerateDailyReport(from, dailyResult.Value!);
                fileName = $"daily-report-{from:yyyy-MM-dd}.docx";
                break;
            case "status-breakdown":
                var statusResult = await _mediator.Send(new GetStatusBreakdownQuery(from, to));
                if (!statusResult.IsSuccess) return BadRequest(new { error = statusResult.Error });
                fileBytes = _reportService.GenerateStatusBreakdownReport(from, to, statusResult.Value!);
                fileName = $"status-breakdown-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.docx";
                break;
            case "cancelled":
                var cancelledResult = await _mediator.Send(new GetCancelledReportQuery(from, to));
                if (!cancelledResult.IsSuccess) return BadRequest(new { error = cancelledResult.Error });
                fileBytes = _reportService.GenerateCancelledReport(from, to, cancelledResult.Value!);
                fileName = $"cancelled-report-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.docx";
                break;
            default:
                return BadRequest(new { error = "Unknown report type." });
        }

        return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
    }
}
