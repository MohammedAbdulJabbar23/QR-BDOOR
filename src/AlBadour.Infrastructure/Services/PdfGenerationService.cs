using System.Reflection;
using AlBadour.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AlBadour.Infrastructure.Services;

public class PdfGenerationService : IDocumentGenerationService
{
    private readonly byte[] _letterheadBytes;
    private readonly byte[] _logoBytes;
    private readonly byte[] _phoneIconBytes;

    public PdfGenerationService()
    {
        var assembly = Assembly.GetExecutingAssembly();
        _letterheadBytes = LoadResource(assembly, "AlBadour.Infrastructure.Resources.Images.letterhead.jpg");
        _logoBytes = LoadResource(assembly, "AlBadour.Infrastructure.Resources.Images.bdoor_logo.png");
        _phoneIconBytes = LoadResource(assembly, "AlBadour.Infrastructure.Resources.Images.phone_icon.png");
    }

    private static byte[] LoadResource(Assembly assembly, string name)
    {
        using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Resource not found: {name}");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public byte[] GenerateDocument(DocumentGenerationData data)
    {
        bool isAdministrativeLetter = data.DocumentTypeNameEn.Equals("Administrative Letter", StringComparison.OrdinalIgnoreCase);
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily("Noto Sans Arabic").FontSize(11));
                page.ContentFromRightToLeft();

                if (isAdministrativeLetter)
                {
                    page.Header().Height(95)
                        .Element(header => ComposeAdminLetterHeader(header));
                    page.Content().PaddingTop(20).PaddingHorizontal(50)
                        .Element(content => ComposeAdminLetterBody(content, data));
                    page.Footer().PaddingHorizontal(50)
                        .Element(footer => ComposeAdminLetterFooter(footer));
                }
                else
                {
                    page.Header().Height(95)
                        .Element(header => ComposeMedicalReportHeader(header));
                    page.Content().PaddingTop(20).PaddingHorizontal(50)
                        .Element(content => ComposeBodyDispatch(content, data));
                    page.Footer()
                        .Element(footer => ComposeSignatureSection(footer, data));
                }
            });
        });

        return document.GeneratePdf();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Shared red contact footer strip (matches letterhead design)
    // ─────────────────────────────────────────────────────────────────────────

    private void ComposeContactStrip(ColumnDescriptor col)
    {
        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
        col.Item().PaddingTop(5).PaddingBottom(8).Row(row =>
        {
            // Right side (Arabic address/website/email) — first item in RTL = visually right
            row.RelativeItem().Column(arabic =>
            {
                arabic.Item().AlignRight()
                    .Text("بغداد - الكرادة - تقاطع بدالة العلوية")
                    .FontSize(8).FontColor(Colors.Red.Darken2);
                arabic.Item().AlignRight()
                    .Text("www.albudoor-hospital.com")
                    .FontSize(8).FontColor(Colors.Red.Darken2);
                arabic.Item().AlignRight()
                    .Text("info@albudoor-hospital.iq")
                    .FontSize(8).FontColor(Colors.Red.Darken2);
            });

            // Middle: extension number 6177
            row.ConstantItem(60).AlignMiddle().Column(mid =>
            {
                mid.Item().AlignCenter().Row(r =>
                {
                    r.AutoItem().Width(10).Height(10).Image(_phoneIconBytes);
                    r.AutoItem().PaddingLeft(3)
                        .Text("6177").FontSize(9).Bold().FontColor(Colors.Red.Darken2);
                });
            });

            // Left side (phone numbers with icons) — last item in RTL = visually left
            row.RelativeItem().Column(phones =>
            {
                phones.Item().AlignLeft().Row(r =>
                {
                    r.AutoItem().Width(9).Height(9).Image(_phoneIconBytes);
                    r.AutoItem().PaddingLeft(3)
                        .Text("+964770000422").FontSize(8).FontColor(Colors.Red.Darken2);
                });
                phones.Item().PaddingTop(2).AlignLeft().Row(r =>
                {
                    r.AutoItem().Width(9).Height(9).Image(_phoneIconBytes);
                    r.AutoItem().PaddingLeft(3)
                        .Text("+9647800004220").FontSize(8).FontColor(Colors.Red.Darken2);
                });
            });
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ADMINISTRATIVE LETTER
    // ─────────────────────────────────────────────────────────────────────────

    private void ComposeAdminLetterHeader(IContainer container)
    {
        container.PaddingTop(16).PaddingHorizontal(50).Column(col =>
        {
            col.Item().Row(row =>
            {
                // Hospital name (right in RTL)
                row.RelativeItem().AlignRight().Column(textCol =>
                {
                    textCol.Item().Text("مستشفى البدور")
                        .FontSize(18).Bold().FontColor(Colors.Red.Darken2);
                    textCol.Item().Text("للجراحات التخصصية")
                        .FontSize(12).Bold();
                });

                // Logo (left in RTL)
                row.ConstantItem(60).Height(60).PaddingRight(8)
                    .Image(_logoBytes);
            });

            col.Item().PaddingTop(8).LineHorizontal(0.8f).LineColor(Colors.Grey.Lighten1);
        });
    }

    private void ComposeMedicalReportHeader(IContainer container)
    {
        container.PaddingTop(16).PaddingHorizontal(50).Column(col =>
        {
            col.Item().Row(row =>
            {
                // Arabic name (right in RTL)
                row.RelativeItem().AlignRight().Column(ar =>
                {
                    ar.Item().Text("مستشفى البدور")
                        .FontSize(16).Bold().FontColor(Colors.Red.Darken2);
                    ar.Item().Text("للجراحات التخصصية")
                        .FontSize(10).Bold().FontColor(Colors.Grey.Darken2);
                });

                // Logo (center)
                row.ConstantItem(64).Height(64).Image(_logoBytes);

                // English name (left in RTL)
                row.RelativeItem().AlignLeft().Column(en =>
                {
                    en.Item().Text("ALBUDOOR HOSPITAL")
                        .FontSize(12).Bold().FontColor(Colors.Red.Darken2);
                    en.Item().Text("For Specialized Surgeries")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                });
            });

            col.Item().PaddingTop(8).LineHorizontal(0.8f).LineColor(Colors.Grey.Lighten1);
        });
    }

    private static void ComposeAdminLetterBody(IContainer container, DocumentGenerationData data)
    {
        container.Column(col =>
        {
            // ── Document number/date + QR ────────────────────────────────────
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(info =>
                {
                    info.Item().Text($"العدد:   {data.DocumentNumber}")
                        .FontSize(12).Bold();
                    info.Item().PaddingTop(4).Text($"التاريخ: {data.IssuedAt:d/M/yyyy}")
                        .FontSize(12).Bold();
                });

                row.AutoItem().Width(110).Height(110)
                    .Image(data.QrCodeImageBytes);
            });

            col.Item().PaddingTop(10);

            // ── Recipient ────────────────────────────────────────────────────
            foreach (var line in data.RecipientEntity.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                col.Item().Text($"الى / {line.Trim()}").FontSize(11);
            }

            col.Item().PaddingTop(4);

            // ── Subject ──────────────────────────────────────────────────────
            col.Item().Text(string.IsNullOrWhiteSpace(data.Subject)
                ? "م/ "
                : $"م/ {data.Subject}").FontSize(11);

            col.Item().PaddingTop(6);

            // ── Greeting ─────────────────────────────────────────────────────
            col.Item().Text("تحية طيبة وبعد،،،").FontSize(11);

            // ── Patient reference ────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(data.PatientName))
            {
                col.Item().PaddingTop(6).Text(t =>
                {
                    t.Span("بخصوص المريض/ة: ").FontSize(11).Bold();
                    t.Span(data.PatientName).FontSize(11);
                });
            }

            // ── Body text ────────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(data.DocumentBody))
            {
                col.Item().PaddingTop(8).Text(data.DocumentBody)
                    .FontSize(11).LineHeight(1.4f);
            }

            col.Item().PaddingTop(16);

            // ── Closing ──────────────────────────────────────────────────────
            col.Item().Text("يرجى التفضل بالإطلاع مع الشكر والتقدير..")
                .FontSize(11);
        });
    }

    private void ComposeAdminLetterFooter(IContainer container)
    {
        container.Column(col =>
        {
            // ── Signature / Stamp ────────────────────────────────────────────
            col.Item().PaddingTop(8).PaddingBottom(10).Row(row =>
            {
                // Name + Signature (right in RTL)
                row.RelativeItem().Column(sigCol =>
                {
                    sigCol.Item().Text("الاسم:").FontSize(10).Bold();
                    sigCol.Item().PaddingTop(10).Width(170)
                        .LineHorizontal(0.5f).LineColor(Colors.Grey.Darken1);

                    sigCol.Item().PaddingTop(12).Text("التوقيع:").FontSize(10).Bold();
                    sigCol.Item().PaddingTop(10).Width(170)
                        .LineHorizontal(0.5f).LineColor(Colors.Grey.Darken1);
                });

                // Stamp (left in RTL) — no border
                row.ConstantItem(110).Column(stampCol =>
                {
                    stampCol.Item().AlignCenter().Text("الختم").FontSize(10).Bold();
                    stampCol.Item().Height(50);
                });
            });

            // ── Red contact strip (matches letterhead design) ─────────────
            ComposeContactStrip(col);
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MEDICAL REPORT / FREE-FORM
    // ─────────────────────────────────────────────────────────────────────────

    private void ComposeBodyDispatch(IContainer container, DocumentGenerationData data)
    {
        bool hasMedicalFields = !string.IsNullOrEmpty(data.PatientGender)
            || !string.IsNullOrEmpty(data.PatientAge)
            || !string.IsNullOrEmpty(data.PatientProfession)
            || !string.IsNullOrEmpty(data.AdmissionDate)
            || !string.IsNullOrEmpty(data.DischargeDate)
            || !string.IsNullOrEmpty(data.LeaveGranted);

        if (hasMedicalFields)
            ComposeMedicalReport(container, data);
        else
            ComposeFreeFormLetter(container, data);
    }

    private static void ComposeMedicalReport(IContainer container, DocumentGenerationData data)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(info =>
                {
                    info.Item().Text($"العدد:   {data.DocumentNumber}")
                        .FontSize(12).Bold();
                    info.Item().Text($"التاريخ: {data.IssuedAt:d/M/yyyy}")
                        .FontSize(12).Bold();
                });

                row.AutoItem().Width(90).Height(90)
                    .Image(data.QrCodeImageBytes);
            });

            col.Item().PaddingVertical(3);

            col.Item().AlignCenter().Text($"إلى/ {data.RecipientEntity}").FontSize(12);
            col.Item().AlignCenter().PaddingBottom(3).Text($"م/ {data.Subject}")
                .FontSize(12).Bold();

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(110);
                    columns.RelativeColumn();
                });

                AddTableRow(table, "اسم المريض:", data.PatientName);

                if (!string.IsNullOrEmpty(data.PatientNameEn))
                    AddTableRow(table, "Patient Name:", data.PatientNameEn);
                if (!string.IsNullOrEmpty(data.PatientGender))
                    AddTableRow(table, "الجنس:", data.PatientGender);
                if (!string.IsNullOrEmpty(data.PatientProfession))
                    AddTableRow(table, "المهنة:", data.PatientProfession);
                if (!string.IsNullOrEmpty(data.PatientAge))
                    AddTableRow(table, "العمر:", data.PatientAge);
                if (!string.IsNullOrEmpty(data.AdmissionDate))
                    AddTableRow(table, "تاريخ الدخول:", data.AdmissionDate);
                if (!string.IsNullOrEmpty(data.DischargeDate))
                    AddTableRow(table, "تاريخ الخروج:", data.DischargeDate);
                if (!string.IsNullOrEmpty(data.DocumentBody))
                    AddTableRow(table, "التشخيص:", data.DocumentBody);
                if (!string.IsNullOrEmpty(data.LeaveGranted))
                    AddTableRow(table, "الاجازة الممنوحة:", data.LeaveGranted);
            });
        });
    }

    private static void ComposeFreeFormLetter(IContainer container, DocumentGenerationData data)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(info =>
                {
                    info.Item().Text($"العدد: {data.DocumentNumber}")
                        .FontSize(12).Bold();
                    info.Item().Text($"التاريخ: {data.IssuedAt:d/M/yyyy}")
                        .FontSize(12).Bold();
                });

                row.AutoItem().Width(90).Height(90)
                    .Image(data.QrCodeImageBytes);
            });

            col.Item().PaddingVertical(3);

            foreach (var line in data.RecipientEntity.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                col.Item().Text($"الى /{line.Trim()}").FontSize(11);

            col.Item().PaddingVertical(2);
            col.Item().Text($"م/{data.Subject}").FontSize(12).Bold();
            col.Item().PaddingTop(2).Text("تحية طيبة...").FontSize(11);

            if (!string.IsNullOrWhiteSpace(data.PatientName))
            {
                col.Item().PaddingTop(2).Text(t =>
                {
                    t.Span("بخصوص المريض/ة: ").FontSize(11).Bold();
                    t.Span(data.PatientName).FontSize(11);
                    if (!string.IsNullOrEmpty(data.PatientNameEn))
                        t.Span($" / {data.PatientNameEn}").FontSize(11);
                });
            }

            if (!string.IsNullOrEmpty(data.DocumentBody))
                col.Item().PaddingTop(4).Text(data.DocumentBody).FontSize(11).LineHeight(1.3f);

            col.Item().PaddingTop(4);
            col.Item().Text("يرجى التفضل بالإطلاع مع الشكر والتقدير..").FontSize(11);
        });
    }

    private static void AddTableRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(3)
            .Text(label).FontSize(11).Bold();
        table.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(3)
            .Text(value).FontSize(11);
    }

    /// <summary>
    /// Footer for medical/free-form documents.
    /// Wraps content in white background to cover the letterhead image's wrong contact info,
    /// then draws the correct contact strip in red.
    /// </summary>
    private void ComposeSignatureSection(IContainer container, DocumentGenerationData data)
    {
        _ = data;
        container.Column(col =>
        {
            // ── Signature / Stamp ────────────────────────────────────────────
            col.Item().PaddingHorizontal(50).PaddingTop(8).PaddingBottom(6).Row(row =>
            {
                row.RelativeItem().Column(sigCol =>
                {
                    sigCol.Item().Text("الاسم / Name").FontSize(9).Bold();
                    sigCol.Item().PaddingTop(10).Width(150)
                        .LineHorizontal(0.5f).LineColor(Colors.Grey.Darken1);

                    sigCol.Item().PaddingTop(6).Text("التوقيع / Signature").FontSize(9).Bold();
                    sigCol.Item().PaddingTop(10).Width(150)
                        .LineHorizontal(0.5f).LineColor(Colors.Grey.Darken1);
                });

                row.AutoItem().AlignLeft().Width(100).Column(stampCol =>
                {
                    stampCol.Item().Text("الختم / Stamp").FontSize(9).Bold();
                    stampCol.Item().PaddingTop(3).Height(40).Width(95)
                        .Border(0.5f).BorderColor(Colors.Grey.Lighten1);
                });
            });

            // ── Red contact strip — padding inside so white background is full-width ──
            col.Item().PaddingHorizontal(50).Column(inner => ComposeContactStrip(inner));
        });
    }
}
