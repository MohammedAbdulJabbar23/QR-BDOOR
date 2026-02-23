using AlBadour.Application.Common.Interfaces;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AlBadour.Infrastructure.Services;

public class ReportGenerationService : IReportGenerationService
{
    public byte[] GenerateDailyReport(DateTime date, object data)
    {
        return GenerateWordDocument($"Daily Report - {date:yyyy-MM-dd}", data);
    }

    public byte[] GenerateMedicalSummaryReport(DateTime from, DateTime to, object data)
    {
        return GenerateWordDocument($"Medical Reports Summary - {from:yyyy-MM-dd} to {to:yyyy-MM-dd}", data);
    }

    public byte[] GenerateStatusBreakdownReport(DateTime from, DateTime to, object data)
    {
        return GenerateWordDocument($"Status Breakdown - {from:yyyy-MM-dd} to {to:yyyy-MM-dd}", data);
    }

    public byte[] GenerateCancelledReport(DateTime from, DateTime to, object data)
    {
        return GenerateWordDocument($"Cancelled/Revoked Documents - {from:yyyy-MM-dd} to {to:yyyy-MM-dd}", data);
    }

    private static byte[] GenerateWordDocument(string title, object data)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Title
            var titleParagraph = body.AppendChild(new Paragraph());
            var titleRun = titleParagraph.AppendChild(new Run());
            titleRun.AppendChild(new RunProperties(new Bold()));
            titleRun.AppendChild(new Text(title));

            // Add spacing
            body.AppendChild(new Paragraph());

            // Content - serialize the data object
            var content = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                var para = body.AppendChild(new Paragraph());
                var run = para.AppendChild(new Run());
                run.AppendChild(new Text(line) { Space = SpaceProcessingModeValues.Preserve });
            }
        }

        return stream.ToArray();
    }
}
