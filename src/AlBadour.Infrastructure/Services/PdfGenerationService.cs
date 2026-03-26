using System.Reflection;
using AlBadour.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AlBadour.Infrastructure.Services;

public class PdfGenerationService : IDocumentGenerationService
{
    private readonly byte[] _templateBytes;
    private readonly byte[] _phoneIconBytes;
    private readonly byte[]? _zaidSignatureBytes;

    public PdfGenerationService()
    {
        var assembly = Assembly.GetExecutingAssembly();
        _templateBytes = LoadResource(assembly, "AlBadour.Infrastructure.Resources.Images.documentTemplate.jpg");
        _phoneIconBytes = LoadResource(assembly, "AlBadour.Infrastructure.Resources.Images.phone_icon.png");
        _zaidSignatureBytes = TryLoadResource(assembly, "AlBadour.Infrastructure.Resources.Images.zaidSignature.png");
    }

    private static byte[]? TryLoadResource(Assembly assembly, string name)
    {
        using var stream = assembly.GetManifestResourceStream(name);
        if (stream is null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
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
        bool isAdminLetter = data.DocumentTypeNameEn
            .Equals("Administrative Letter", StringComparison.OrdinalIgnoreCase);
        bool hasTable = data.DocumentTypeNameEn
            .Contains("with Table", StringComparison.OrdinalIgnoreCase);
        bool isEnglish = string.Equals(data.Language, "English", StringComparison.OrdinalIgnoreCase);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily("Noto Sans Arabic").FontSize(11));

                if (!isEnglish)
                    page.ContentFromRightToLeft();

                // Template image covers the entire page (header, watermark, footer)
                page.Background().Image(_templateBytes);

                // Reserve space occupied by the template's header
                page.Header().Height(90);

                // Template footer area — overlay phone icon + 6177 in the center
                var phoneIcon = _phoneIconBytes;
                page.Footer().Height(68).AlignCenter().AlignMiddle().Row(row =>
                {
                    row.AutoItem().Width(13).Height(13).AlignMiddle().Image(phoneIcon);
                    row.AutoItem().PaddingLeft(4).AlignMiddle()
                        .Text("6177").FontSize(12).Bold().FontColor(Colors.Red.Darken2);
                });

                // All document content goes in the middle white area
                page.Content().PaddingHorizontal(50).PaddingVertical(8).Column(col =>
                {
                    if (isAdminLetter)
                        ComposeAdminLetterBody(col.Item(), data, isEnglish);
                    else if (hasTable)
                        ComposeMedicalReport(col.Item(), data, isEnglish);
                    else
                        ComposeFreeFormLetter(col.Item(), data, isEnglish);

                    col.Item().PaddingTop(8).PaddingHorizontal(12).Element(c => ComposeSignatureBlock(c, data.TreatingPhysicianName, isEnglish, data.IncludeDirectorSignature ? _zaidSignatureBytes : null, !isAdminLetter));
                });
            });
        });

        return document.GeneratePdf();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SIGNATURE BLOCK
    // ─────────────────────────────────────────────────────────────────────────

    private static void ComposeSignatureBlock(IContainer container, string? treatingPhysicianName, bool isEnglish, byte[]? directorSignatureBytes, bool showTreatingPhysician = true)
    {
        string physicianLabel = isEnglish ? "Treating Physician" : "الطبيب المعالج";
        string directorLabel = isEnglish ? "Hospital Director" : "مدير المستشفى";
        string directorName = isEnglish ? "Dr. Zaid Saleh Yaseen" : "د.زيد صالح ياسين";

        if (!showTreatingPhysician)
        {
            container.AlignLeft().Column(col =>
            {
                if (directorSignatureBytes is not null)
                    col.Item().AlignLeft().TranslateX(-15).ScaleHorizontal(1.5f).Width(80).Image(directorSignatureBytes);
                else
                    col.Item().Height(45);
                col.Item().PaddingTop(4).AlignCenter().Text(directorLabel).FontSize(10).Bold();
                col.Item().PaddingTop(2).AlignCenter().Text(directorName).FontSize(10);
            });
            return;
        }

        // In RTL: AutoItem order = right→left. In LTR: left→right.
        // Desired visual: Treating Physician on right, Hospital Director on far left.
        // RTL: [Physician(right)] [Spacer] [Director(left)]
        // LTR: [Director(left)] [Spacer] [Physician(right)]

        container.Row(row =>
        {
            if (isEnglish)
            {
                // LTR: Director on far left, Physician on far right
                row.RelativeItem().Column(col =>
                {
                    if (directorSignatureBytes is not null)
                        col.Item().AlignLeft().TranslateX(-15).ScaleHorizontal(1.5f).Width(80).Image(directorSignatureBytes);
                    else
                        col.Item().Height(74);
                    col.Item().PaddingTop(4).AlignLeft()
                        .Text(directorLabel).FontSize(10).Bold();
                    col.Item().PaddingTop(2).AlignLeft()
                        .Text(directorName).FontSize(10);
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Height(74);
                    col.Item().PaddingTop(4).AlignRight()
                        .Text(physicianLabel).FontSize(10).Bold();
                    if (!string.IsNullOrWhiteSpace(treatingPhysicianName))
                        col.Item().PaddingTop(2).AlignRight()
                            .Text(treatingPhysicianName).FontSize(10);
                });
            }
            else
            {
                // RTL: Physician on far right (first item), Director on far left (last item)
                row.RelativeItem().Column(col =>
                {
                    col.Item().Height(74);
                    col.Item().PaddingTop(4).AlignRight()
                        .Text(physicianLabel).FontSize(10).Bold();
                    if (!string.IsNullOrWhiteSpace(treatingPhysicianName))
                        col.Item().PaddingTop(2).AlignRight()
                            .Text(treatingPhysicianName).FontSize(10);
                });

                row.RelativeItem().Column(col =>
                {
                    if (directorSignatureBytes is not null)
                        col.Item().AlignLeft().TranslateX(-15).ScaleHorizontal(1.5f).Width(80).Image(directorSignatureBytes);
                    else
                        col.Item().Height(74);
                    col.Item().PaddingTop(4).AlignLeft()
                        .Text(directorLabel).FontSize(10).Bold();
                    col.Item().PaddingTop(2).AlignLeft()
                        .Text(directorName).FontSize(10);
                });
            }
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ADMINISTRATIVE LETTER BODY
    // ─────────────────────────────────────────────────────────────────────────

    private static void ComposeAdminLetterBody(IContainer container, DocumentGenerationData data, bool isEnglish)
    {
        container.Column(col =>
        {
            // ── Document number/date + QR ────────────────────────────────────
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(info =>
                {
                    string numLabel = isEnglish ? "Doc No.:" : "العدد:";
                    string dateLabel = isEnglish ? "Date:" : "التاريخ:";
                    info.Item().Text($"{dateLabel} {data.IssuedAt:d/M/yyyy}").FontSize(12).Bold();
                    info.Item().PaddingTop(4).Text($"{numLabel} {data.DocumentNumber}").FontSize(12).Bold();
                });

                row.AutoItem().Width(110).Height(110)
                    .Image(data.QrCodeImageBytes);
            });

            col.Item().PaddingTop(10);

            // ── Recipient ────────────────────────────────────────────────────
            foreach (var line in data.RecipientEntity.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                string toPrefix = isEnglish ? "To: " : "الى / ";
                col.Item().AlignCenter().Text($"{toPrefix}{line.Trim()}").FontSize(11).Bold();
            }

            col.Item().PaddingTop(4);

            // ── Subject ──────────────────────────────────────────────────────
            string subjectPrefix = isEnglish ? "Subject: " : "م/ ";
            col.Item().AlignCenter().Text(string.IsNullOrWhiteSpace(data.Subject)
                ? subjectPrefix
                : $"{subjectPrefix}{data.Subject}").FontSize(11).Bold();

            col.Item().PaddingTop(6);

            // ── Greeting ─────────────────────────────────────────────────────
            string greeting = isEnglish ? "Greetings," : "تحية طيبة وبعد،،،";
            col.Item().Text(greeting).FontSize(11);

            // ── Patient reference ────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(data.PatientName))
            {
                string patientRef = isEnglish ? "Regarding Patient: " : "بخصوص المريض/ة: ";
                col.Item().PaddingTop(6).Text(t =>
                {
                    t.Span(patientRef).FontSize(11).Bold();
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
            string closing = isEnglish ? "Sincerely," : "يرجى التفضل بالإطلاع مع الشكر والتقدير..";
            col.Item().AlignCenter().Text(closing).FontSize(11);
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MEDICAL REPORT WITH TABLE
    // ─────────────────────────────────────────────────────────────────────────

    private static void ComposeMedicalReport(IContainer container, DocumentGenerationData data, bool isEnglish)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(info =>
                {
                    string numLabel = isEnglish ? "Doc No.:" : "العدد:  ";
                    string dateLabel = isEnglish ? "Date:   " : "التاريخ:";
                    info.Item().Text($"{numLabel}   {data.DocumentNumber}")
                        .FontSize(12).Bold();
                    info.Item().Text($"{dateLabel} {data.IssuedAt:d/M/yyyy}")
                        .FontSize(12).Bold();
                });

                row.AutoItem().Width(90).Height(90)
                    .Image(data.QrCodeImageBytes);
            });

            col.Item().PaddingVertical(2);

            string toPrefix2 = isEnglish ? "To/ " : "إلى/ ";
            col.Item().AlignCenter().Text(t =>
            {
                t.Span(toPrefix2).FontSize(11).Bold();
                t.Span(data.RecipientEntity).FontSize(11);
            });
            string subjectText = isEnglish ? "Re/ Medical Report" : "م/ تقرير طبي";
            col.Item().AlignCenter().PaddingBottom(2).Text(subjectText)
                .FontSize(11).Bold();

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });

                if (isEnglish)
                {
                    AddTableRow(table, "Patient Name:", data.PatientName);
                    if (!string.IsNullOrEmpty(data.PatientNameEn))
                        AddTableRow(table, "Patient Name (En):", data.PatientNameEn);
                    if (!string.IsNullOrEmpty(data.PatientGender))
                        AddTableRow(table, "Gender:", data.PatientGender);
                    if (!string.IsNullOrEmpty(data.PatientProfession))
                        AddTableRow(table, "Profession:", data.PatientProfession);
                    if (!string.IsNullOrEmpty(data.PatientAge))
                        AddTableRow(table, "Age:", data.PatientAge);
                    if (!string.IsNullOrEmpty(data.AdmissionDate))
                        AddTableRow(table, "Admission Date:", data.AdmissionDate);
                    if (!string.IsNullOrEmpty(data.DischargeDate))
                        AddTableRow(table, "Discharge Date:", data.DischargeDate);
                    if (!string.IsNullOrEmpty(data.DocumentBody))
                        AddTableRow(table, "Diagnosis:", data.DocumentBody);
                    if (!string.IsNullOrEmpty(data.LeaveGranted))
                        AddTableRow(table, "Leave Granted:", data.LeaveGranted);
                }
                else
                {
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
                }
            });
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FREE-FORM LETTER (medical report without table)
    // ─────────────────────────────────────────────────────────────────────────

    private static void ComposeFreeFormLetter(IContainer container, DocumentGenerationData data, bool isEnglish)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(info =>
                {
                    string numLabel = isEnglish ? "Doc No.:" : "العدد:";
                    string dateLabel = isEnglish ? "Date:   " : "التاريخ:";
                    info.Item().Text($"{numLabel} {data.DocumentNumber}")
                        .FontSize(12).Bold();
                    info.Item().Text($"{dateLabel} {data.IssuedAt:d/M/yyyy}")
                        .FontSize(12).Bold();
                });

                row.AutoItem().Width(90).Height(90)
                    .Image(data.QrCodeImageBytes);
            });

            col.Item().PaddingVertical(3);

            string toPrefix = isEnglish ? "To / " : "الى / ";
            foreach (var line in data.RecipientEntity.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                col.Item().AlignCenter().Text(t =>
                {
                    t.Span(toPrefix).FontSize(11).Bold();
                    t.Span(line.Trim()).FontSize(11);
                });

            col.Item().PaddingVertical(2);
            string subjectText2 = isEnglish ? "Re/ Medical Report" : "م/ تقرير طبي";
            col.Item().AlignCenter().Text(subjectText2).FontSize(12).Bold();

            string greeting = isEnglish ? "Greetings," : "تحية طيبة...";
            col.Item().PaddingTop(2).Text(greeting).FontSize(11);

            if (!string.IsNullOrWhiteSpace(data.PatientName))
            {
                string patientRef = isEnglish ? "Regarding Patient: " : "بخصوص المريض/ة: ";
                col.Item().PaddingTop(2).Text(t =>
                {
                    t.Span(patientRef).FontSize(11).Bold();
                    t.Span(data.PatientName).FontSize(11);
                    if (!string.IsNullOrEmpty(data.PatientNameEn))
                        t.Span($" / {data.PatientNameEn}").FontSize(11);
                });
            }

            if (!string.IsNullOrEmpty(data.DocumentBody))
                col.Item().PaddingTop(4).Text(data.DocumentBody).FontSize(11).LineHeight(1.3f);

            col.Item().PaddingTop(4);
            string closing = isEnglish ? "Sincerely," : "يرجى التفضل بالإطلاع مع الشكر والتقدير..";
            col.Item().AlignCenter().Text(closing).FontSize(11);
        });
    }

    private static void AddTableRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(2)
            .Text(label).FontSize(10).Bold();
        table.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(2)
            .Text(value).FontSize(10);
    }
}
