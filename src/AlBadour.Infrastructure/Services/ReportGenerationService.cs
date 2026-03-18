using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Features.DocumentRequests.DTOs;
using AlBadour.Application.Features.IssuedDocuments.DTOs;
using AlBadour.Application.Features.Reports.DTOs;
using System.Text;

namespace AlBadour.Infrastructure.Services;

public class ReportGenerationService : IReportGenerationService
{
    public byte[] GenerateDailyReport(DateTime date, DailyReportDto data)
    {
        return BuildCsv(new[]
        {
            new[] { "Metric", "Value" },
            new[] { "Date", date.ToString("yyyy-MM-dd") },
            new[] { "Total Requests", data.TotalRequests.ToString() },
            new[] { "Pending Requests", data.PendingRequests.ToString() },
            new[] { "Completed Requests", data.CompletedRequests.ToString() },
            new[] { "Rejected Requests", data.RejectedRequests.ToString() },
            new[] { "Documents Issued", data.DocumentsIssued.ToString() },
            new[] { "Documents Archived", data.DocumentsArchived.ToString() }
        });
    }

    public byte[] GenerateStatusBreakdownReport(DateTime from, DateTime to, IEnumerable<StatusBreakdownDto> data)
    {
        var rows = new List<string[]>
        {
            new[] { "From", from.ToString("yyyy-MM-dd") },
            new[] { "To", to.ToString("yyyy-MM-dd") },
            new[] { string.Empty, string.Empty },
            new[] { "Status", "Count" }
        };

        rows.AddRange(data.Select(item => new[] { item.Status, item.Count.ToString() }));
        return BuildCsv(rows);
    }

    public byte[] GenerateCancelledReport(DateTime from, DateTime to, IEnumerable<CancelledDocumentDto> data)
    {
        var rows = new List<string[]>
        {
            new[] { "From", from.ToString("yyyy-MM-dd") },
            new[] { "To", to.ToString("yyyy-MM-dd") },
            new[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty },
            new[] { "Document Number", "Patient Name", "Revocation Reason", "Revoked At", "Replacement Document Number" }
        };

        rows.AddRange(data.Select(item => new[]
        {
            item.DocumentNumber,
            item.PatientName,
            item.RevocationReason ?? string.Empty,
            item.RevokedAt?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty,
            item.ReplacementDocumentNumber ?? string.Empty
        }));

        return BuildCsv(rows);
    }

    public byte[] GenerateRequestsExtractReport(DateTime? from, DateTime? to, IEnumerable<RequestDto> data)
    {
        var rows = new List<string[]>
        {
            new[] { "From", from?.ToString("yyyy-MM-dd") ?? string.Empty },
            new[] { "To", to?.ToString("yyyy-MM-dd") ?? string.Empty },
            new[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty },
            new[] { "Request ID", "Patient Name", "Recipient Entity", "Document Type", "Status", "Created By", "Assigned To", "Created At", "Topic / Notes" }
        };

        rows.AddRange(data.Select(item => new[]
        {
            item.Id.ToString(),
            item.PatientName,
            item.RecipientEntity,
            item.DocumentTypeNameEn,
            item.Status,
            item.CreatedByName,
            item.AssignedToName ?? string.Empty,
            item.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
            item.Notes ?? string.Empty
        }));

        return BuildCsv(rows);
    }

    public byte[] GenerateDocumentsExtractReport(DateTime? from, DateTime? to, IEnumerable<DocumentDto> data)
    {
        var rows = new List<string[]>
        {
            new[] { "From", from?.ToString("yyyy-MM-dd") ?? string.Empty },
            new[] { "To", to?.ToString("yyyy-MM-dd") ?? string.Empty },
            new[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty },
            new[] { "Document ID", "Document Number", "Patient Name", "Recipient Entity", "Document Type", "Status", "Issued By", "Issued At", "Archived At" }
        };

        rows.AddRange(data.Select(item => new[]
        {
            item.Id.ToString(),
            item.DocumentNumber,
            item.PatientName,
            item.RecipientEntity,
            item.DocumentTypeNameEn,
            item.Status,
            item.IssuedByName,
            item.IssuedAt.ToString("yyyy-MM-dd HH:mm"),
            item.ArchivedAt?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty
        }));

        return BuildCsv(rows);
    }

    private static byte[] BuildCsv(IEnumerable<string[]> rows)
    {
        var builder = new StringBuilder();

        foreach (var row in rows)
            builder.AppendLine(string.Join(",", row.Select(EscapeCsv)));

        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = Encoding.UTF8.GetBytes(builder.ToString());
        return preamble.Concat(bytes).ToArray();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var escaped = value.Replace("\"", "\"\"");
        return escaped.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0
            ? $"\"{escaped}\""
            : escaped;
    }
}
