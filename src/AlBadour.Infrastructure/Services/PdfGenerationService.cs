using System.Reflection;
using AlBadour.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AlBadour.Infrastructure.Services;

public class PdfGenerationService : IPdfGenerationService
{
    private readonly byte[] _logoBytes;

    public PdfGenerationService()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var logoStream = assembly.GetManifestResourceStream(
            "AlBadour.Infrastructure.Resources.Images.bdoor_logo.png")
            ?? throw new InvalidOperationException("Logo resource not found.");
        using var ms = new MemoryStream();
        logoStream.CopyTo(ms);
        _logoBytes = ms.ToArray();
    }

    public byte[] GenerateDocumentPdf(PdfDocumentData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginTop(30);
                page.MarginBottom(30);
                page.MarginHorizontal(40);
                page.DefaultTextStyle(x => x.FontFamily("Noto Sans Arabic").FontSize(11));
                page.ContentFromRightToLeft();

                page.Header().Element(header => ComposeHeader(header, data));
                page.Content().Element(content => ComposeBodyDispatch(content, data));
                page.Footer().Element(footer => ComposeFooterWithSignature(footer, data));

                page.Background().Element(bg => ComposeWatermark(bg));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, PdfDocumentData data)
    {
        container.Column(col =>
        {
            // Logo + hospital name row
            col.Item().Row(row =>
            {
                row.AutoItem().Width(60).Image(_logoBytes);
                row.RelativeItem().PaddingHorizontal(10).Column(nameCol =>
                {
                    nameCol.Item().Text("مستشفى البدور")
                        .FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                    nameCol.Item().Text("Al-Badour Hospital")
                        .FontSize(12).FontColor(Colors.Blue.Darken1);
                });
            });

            col.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Blue.Darken3);

            // Document type + number + date
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(data.DocumentTypeNameAr)
                        .FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                    c.Item().Text(data.DocumentTypeNameEn)
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                });

                row.AutoItem().AlignLeft().Column(c =>
                {
                    c.Item().Text($"رقم الوثيقة: {data.DocumentNumber}")
                        .FontSize(10).Bold();
                    c.Item().Text($"التاريخ: {data.IssuedAt:yyyy-MM-dd}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2);
                });
            });

            col.Item().PaddingTop(8).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
        });
    }

    private static void ComposeBodyDispatch(IContainer container, PdfDocumentData data)
    {
        switch (data.DocumentTypeNameEn)
        {
            case "Medical Report":
                ComposeMedicalReportBody(container, data);
                break;
            case "Administrative Letter":
                ComposeAdministrativeLetterBody(container, data);
                break;
            default:
                ComposeBody(container, data);
                break;
        }
    }

    private static void ComposeMedicalReportBody(IContainer container, PdfDocumentData data)
    {
        container.PaddingVertical(15).Column(col =>
        {
            // Patient info bordered table
            col.Item().PaddingBottom(10).Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(120);
                    columns.RelativeColumn();
                });

                table.Cell().Element(CellStyle).Text("اسم المريض:").Bold();
                table.Cell().Element(CellStyle).Text(data.PatientName);

                if (!string.IsNullOrEmpty(data.PatientNameEn))
                {
                    table.Cell().Element(CellStyle).Text("Patient Name:").Bold().DirectionFromLeftToRight();
                    table.Cell().Element(CellStyle).Text(data.PatientNameEn).DirectionFromLeftToRight();
                }

                table.Cell().Element(CellStyle).Text("رقم الوثيقة:").Bold();
                table.Cell().Element(CellStyle).Text(data.DocumentNumber);

                table.Cell().Element(CellStyle).Text("التاريخ:").Bold();
                table.Cell().Element(CellStyle).Text(data.IssuedAt.ToString("yyyy-MM-dd"));

                table.Cell().Element(CellStyle).Text("الجهة المستلمة:").Bold();
                table.Cell().Element(CellStyle).Text(data.RecipientEntity);
            });

            // Section heading
            col.Item().PaddingVertical(10).AlignCenter()
                .Text("التقرير الطبي")
                .FontSize(16).Bold().FontColor(Colors.Blue.Darken3);

            // Body text with clinical line-height
            col.Item().PaddingTop(5).Text(data.DocumentBody)
                .FontSize(12).LineHeight(2.0f);

        });

        static IContainer CellStyle(IContainer c) => c.Padding(4);
    }

    private static void ComposeAdministrativeLetterBody(IContainer container, PdfDocumentData data)
    {
        container.PaddingVertical(15).Column(col =>
        {
            // Recipient
            col.Item().PaddingBottom(5).DefaultTextStyle(x => x.FontSize(12)).Text(t =>
            {
                t.Span("إلى / To: ").Bold();
                t.Span(data.RecipientEntity);
            });

            // Subject
            col.Item().PaddingBottom(5).DefaultTextStyle(x => x.FontSize(12)).Text(t =>
            {
                t.Span("الموضوع / Subject: ").Bold();
                t.Span(data.DocumentTypeNameAr);
            });

            // Patient reference
            col.Item().PaddingBottom(10).DefaultTextStyle(x => x.FontSize(11)).Text(t =>
            {
                t.Span("بخصوص المريض/ة: ").Bold();
                t.Span(data.PatientName);
                if (!string.IsNullOrEmpty(data.PatientNameEn))
                {
                    t.Span($" / {data.PatientNameEn}");
                }
            });

            // Separator
            col.Item().PaddingVertical(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            // Body text
            col.Item().PaddingTop(10).Text(data.DocumentBody)
                .FontSize(12).LineHeight(1.8f);

            // Formal closing
            col.Item().PaddingTop(25).Text("وتفضلوا بقبول فائق الاحترام والتقدير")
                .FontSize(11).Bold();
        });
    }

    private static void ComposeBody(IContainer container, PdfDocumentData data)
    {
        container.PaddingVertical(15).Column(col =>
        {
            // Patient info section
            col.Item().PaddingBottom(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Cell().Row(1).Column(1).Element(CellStyle)
                    .Text(t =>
                    {
                        t.Span("اسم المريض: ").Bold();
                        t.Span(data.PatientName);
                    });

                if (!string.IsNullOrEmpty(data.PatientNameEn))
                {
                    table.Cell().Row(1).Column(2).Element(CellStyle)
                        .Text(t =>
                        {
                            t.Span("Patient: ").Bold().DirectionFromLeftToRight();
                            t.Span(data.PatientNameEn).DirectionFromLeftToRight();
                        });
                }

                table.Cell().Row(2).Column(1).ColumnSpan(2).Element(CellStyle)
                    .Text(t =>
                    {
                        t.Span("الجهة المستلمة: ").Bold();
                        t.Span(data.RecipientEntity);
                    });
            });

            // Separator
            col.Item().PaddingVertical(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            // Document body
            col.Item().PaddingTop(10).Text(data.DocumentBody)
                .FontSize(12).LineHeight(1.8f);
        });

        static IContainer CellStyle(IContainer c) => c.Padding(4);
    }

    private void ComposeFooterWithSignature(IContainer container, PdfDocumentData data)
    {
        container.Column(col =>
        {
            // ── Signature & Stamp area ──
            col.Item().LineHorizontal(1).LineColor(Colors.Blue.Darken3);

            col.Item().PaddingTop(10).Row(row =>
            {
                // Right side: Name + Signature
                row.RelativeItem().Column(sigCol =>
                {
                    sigCol.Item().Text("الاسم / Name")
                        .FontSize(10).Bold().FontColor(Colors.Blue.Darken3);

                    // Blank line for manual name writing
                    sigCol.Item().PaddingTop(20).Width(200).LineHorizontal(0.5f).LineColor(Colors.Grey.Darken1);

                    sigCol.Item().PaddingTop(15).Text("التوقيع / Signature")
                        .FontSize(10).Bold().FontColor(Colors.Blue.Darken3);

                    // Blank line for physical signature
                    sigCol.Item().PaddingTop(20).Width(200).LineHorizontal(0.5f).LineColor(Colors.Grey.Darken1);
                });

                // Left side: Stamp box
                row.AutoItem().AlignLeft().Width(150).Column(stampCol =>
                {
                    stampCol.Item().Text("الختم / Stamp")
                        .FontSize(10).Bold().FontColor(Colors.Blue.Darken3);

                    stampCol.Item().PaddingTop(4)
                        .Height(60).Width(140)
                        .Border(0.5f).BorderColor(Colors.Grey.Lighten1);
                });
            });

            // ── QR & Issued-by info ──
            col.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

            col.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"صادرة بواسطة: {data.IssuedByName}")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"تاريخ الإصدار: {data.IssuedAt:yyyy-MM-dd HH:mm}")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(2).Text(data.QrCodeUrl)
                        .FontSize(7).FontColor(Colors.Grey.Medium);
                });

                row.AutoItem().AlignLeft().Width(70).Height(70)
                    .Image(data.QrCodeImageBytes);
            });
        });
    }

    private void ComposeWatermark(IContainer container)
    {
        container.AlignCenter().AlignMiddle()
            .Text("مستشفى البدور")
            .FontSize(72)
            .Bold()
            .FontColor(Colors.Grey.Lighten3)
            .FontFamily("Noto Sans Arabic");
    }
}
