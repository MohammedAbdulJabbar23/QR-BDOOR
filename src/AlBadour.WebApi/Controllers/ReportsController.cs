using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.DocumentRequests.DTOs;
using AlBadour.Application.Features.DocumentRequests.Queries;
using AlBadour.Application.Features.IssuedDocuments.DTOs;
using AlBadour.Application.Features.IssuedDocuments.Queries;
using AlBadour.Application.Features.Reports.Queries;
using AlBadour.Domain.Enums;
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
    public async Task<IActionResult> Export(
        string reportType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? dataset,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] Guid? documentTypeId,
        CancellationToken cancellationToken)
    {
        byte[] fileBytes;
        string fileName;

        switch (reportType.ToLower())
        {
            case "daily":
                if (!from.HasValue)
                    return BadRequest(new { error = "The from date is required." });

                var dailyResult = await _mediator.Send(new GetDailyReportQuery(from.Value), cancellationToken);
                if (!dailyResult.IsSuccess) return ToErrorResponse(dailyResult);
                fileBytes = _reportService.GenerateDailyReport(from.Value, dailyResult.Value!);
                fileName = $"daily-report-{from.Value:yyyy-MM-dd}.csv";
                break;
            case "status-breakdown":
                if (!from.HasValue || !to.HasValue)
                    return BadRequest(new { error = "Both from and to dates are required." });

                var statusResult = await _mediator.Send(new GetStatusBreakdownQuery(from.Value, to.Value), cancellationToken);
                if (!statusResult.IsSuccess) return ToErrorResponse(statusResult);
                fileBytes = _reportService.GenerateStatusBreakdownReport(from.Value, to.Value, statusResult.Value!);
                fileName = $"status-breakdown-{from.Value:yyyy-MM-dd}-to-{to.Value:yyyy-MM-dd}.csv";
                break;
            case "cancelled":
                if (!from.HasValue || !to.HasValue)
                    return BadRequest(new { error = "Both from and to dates are required." });

                var cancelledResult = await _mediator.Send(new GetCancelledReportQuery(from.Value, to.Value), cancellationToken);
                if (!cancelledResult.IsSuccess) return ToErrorResponse(cancelledResult);
                fileBytes = _reportService.GenerateCancelledReport(from.Value, to.Value, cancelledResult.Value!);
                fileName = $"cancelled-report-{from.Value:yyyy-MM-dd}-to-{to.Value:yyyy-MM-dd}.csv";
                break;
            case "extract":
                if (string.Equals(dataset, "requests", StringComparison.OrdinalIgnoreCase))
                {
                    RequestStatus? requestStatus = null;
                    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RequestStatus>(status, true, out var parsedRequestStatus))
                        requestStatus = parsedRequestStatus;

                    var requestsResult = await GetAllRequestsForExportAsync(
                        requestStatus,
                        search,
                        documentTypeId,
                        from,
                        to,
                        cancellationToken);
                    if (!requestsResult.IsSuccess) return ToErrorResponse(requestsResult);

                    fileBytes = _reportService.GenerateRequestsExtractReport(from, to, requestsResult.Value!.Items);
                    fileName = $"requests-extract-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
                    break;
                }

                if (string.Equals(dataset, "documents", StringComparison.OrdinalIgnoreCase))
                {
                    DocumentStatus? documentStatus = null;
                    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DocumentStatus>(status, true, out var parsedDocumentStatus))
                        documentStatus = parsedDocumentStatus;

                    var documentsResult = await GetAllDocumentsForExportAsync(
                        documentStatus,
                        search,
                        documentTypeId,
                        from,
                        to,
                        cancellationToken);
                    if (!documentsResult.IsSuccess) return ToErrorResponse(documentsResult);

                    fileBytes = _reportService.GenerateDocumentsExtractReport(from, to, documentsResult.Value!.Items);
                    fileName = $"documents-extract-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
                    break;
                }

                return BadRequest(new { error = "Unknown extract dataset." });
            default:
                return BadRequest(new { error = "Unknown report type." });
        }

        return File(fileBytes, "text/csv; charset=utf-8", fileName);
    }

    private async Task<Result<PaginatedList<RequestDto>>> GetAllRequestsForExportAsync(
        RequestStatus? status,
        string? search,
        Guid? documentTypeId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken)
    {
        const int pageSize = 500;
        var page = 1;
        var allItems = new List<RequestDto>();
        var totalCount = 0;

        while (true)
        {
            var result = await _mediator.Send(
                new GetAllRequestsQuery(status, search, documentTypeId, from, to, page, pageSize),
                cancellationToken);
            if (!result.IsSuccess)
                return Result.Failure<PaginatedList<RequestDto>>(result.Error!, result.ErrorCode);

            var batch = result.Value!;
            totalCount = batch.TotalCount;
            allItems.AddRange(batch.Items);

            if (allItems.Count >= totalCount || batch.Items.Count == 0)
                break;

            page++;
        }

        return Result.Success(new PaginatedList<RequestDto>(allItems, totalCount, 1, allItems.Count == 0 ? 1 : allItems.Count));
    }

    private async Task<Result<PaginatedList<DocumentDto>>> GetAllDocumentsForExportAsync(
        DocumentStatus? status,
        string? search,
        Guid? documentTypeId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken)
    {
        const int pageSize = 500;
        var page = 1;
        var allItems = new List<DocumentDto>();
        var totalCount = 0;

        while (true)
        {
            var result = await _mediator.Send(
                new GetAllDocumentsQuery(status, search, documentTypeId, from, to, page, pageSize),
                cancellationToken);
            if (!result.IsSuccess)
                return Result.Failure<PaginatedList<DocumentDto>>(result.Error!, result.ErrorCode);

            var batch = result.Value!;
            totalCount = batch.TotalCount;
            allItems.AddRange(batch.Items);

            if (allItems.Count >= totalCount || batch.Items.Count == 0)
                break;

            page++;
        }

        return Result.Success(new PaginatedList<DocumentDto>(allItems, totalCount, 1, allItems.Count == 0 ? 1 : allItems.Count));
    }

    private IActionResult ToErrorResponse(Result result)
    {
        return result.ErrorCode switch
        {
            "FORBIDDEN" => StatusCode(403, new { error = result.Error, code = result.ErrorCode }),
            "NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
            _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
        };
    }
}
