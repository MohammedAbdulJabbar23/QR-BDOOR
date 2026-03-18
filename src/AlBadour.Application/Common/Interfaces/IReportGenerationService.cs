using AlBadour.Application.Features.DocumentRequests.DTOs;
using AlBadour.Application.Features.IssuedDocuments.DTOs;
using AlBadour.Application.Features.Reports.DTOs;

namespace AlBadour.Application.Common.Interfaces;

public interface IReportGenerationService
{
    byte[] GenerateDailyReport(DateTime date, DailyReportDto data);
    byte[] GenerateStatusBreakdownReport(DateTime from, DateTime to, IEnumerable<StatusBreakdownDto> data);
    byte[] GenerateCancelledReport(DateTime from, DateTime to, IEnumerable<CancelledDocumentDto> data);
    byte[] GenerateRequestsExtractReport(DateTime? from, DateTime? to, IEnumerable<RequestDto> data);
    byte[] GenerateDocumentsExtractReport(DateTime? from, DateTime? to, IEnumerable<DocumentDto> data);
}
