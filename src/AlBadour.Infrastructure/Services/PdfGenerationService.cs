using System.Reflection;
using AlBadour.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AlBadour.Infrastructure.Services;

public class PdfGenerationService : IDocumentGenerationService
{
    private readonly byte[] _letterheadBytes;
    private readonly byte[] _phoneIconBytes;
    private readonly byte[] _locationIconBytes;

    public PdfGenerationService()
    {
        var assembly = Assembly.GetExecutingAssembly();
        _letterheadBytes = LoadResource(assembly, "AlBadour.Infrastructure.Resources.Images.letterhead.jpg");
        _phoneIconBytes = LoadResource(assembly, "AlBadour.Infrastructure.Resources.Images.phone_icon.png");
        _locationIconBytes = LoadResource(assembly, "AlBadour.Infrastructure.Resources.Images.location_icon.png");
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
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily("Noto Sans Arabic").FontSize(11));
                page.ContentFromRightToLeft();

                page.Background().Image(_letterheadBytes);

                page.Header().Height(0);

                page.Content().PaddingTop(180).PaddingBottom(120).PaddingHorizontal(50)
                    .Element(content => ComposeBodyDispatch(content, data));

                page.Footer().Height(0);
            });
        });

        return document.GeneratePdf();
    }

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

    private void ComposeMedicalReport(IContainer container, DocumentGenerationData data)
    {
        container.Column(col =>
        {
            // Document number + date
            col.Item().Text($"العدد:   {data.DocumentNumber}")
                .FontSize(16).Bold();
            col.Item().Text($"التاريخ: {data.IssuedAt:d/M/yyyy}")
                .FontSize(16).Bold();

            col.Item().PaddingVertical(8);

            // Recipient
            col.Item().AlignCenter().Text($"إلى/ {data.RecipientEntity}")
                .FontSize(16);

            // Subject
            col.Item().AlignCenter().PaddingBottom(8).Text($"م/ {data.DocumentTypeNameAr}")
                .FontSize(16).Bold();

            // Patient info table
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(130);
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

            // QR + signature
            col.Item().PaddingTop(15);
            col.Item().Element(c => ComposeFooterSection(c, data));
        });
    }

    private void ComposeFreeFormLetter(IContainer container, DocumentGenerationData data)
    {
        container.Column(col =>
        {
            // Document number + date
            col.Item().Text($"العدد: {data.DocumentNumber}")
                .FontSize(16).Bold();
            col.Item().Text($"التاريخ: {data.IssuedAt:d/M/yyyy}")
                .FontSize(16).Bold();

            col.Item().PaddingVertical(8);

            // Recipient (multi-line support)
            foreach (var line in data.RecipientEntity.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                col.Item().Text($"الى /{line.Trim()}").FontSize(14);
            }

            col.Item().PaddingVertical(4);

            // Subject
            col.Item().Text($"م/{data.DocumentTypeNameAr}").FontSize(16).Bold();

            // Greeting
            col.Item().PaddingTop(4).Text("تحية طيبة...").FontSize(14);

            // Patient reference
            col.Item().PaddingTop(4).Text(t =>
            {
                t.Span("بخصوص المريض/ة: ").FontSize(13).Bold();
                t.Span(data.PatientName).FontSize(13);
                if (!string.IsNullOrEmpty(data.PatientNameEn))
                    t.Span($" / {data.PatientNameEn}").FontSize(13);
            });

            // Body text
            if (!string.IsNullOrEmpty(data.DocumentBody))
            {
                col.Item().PaddingTop(8).Text(data.DocumentBody)
                    .FontSize(14).LineHeight(1.8f);
            }

            col.Item().PaddingTop(8);

            // Closing
            col.Item().Text("يرجى التفضل بالإطلاع مع الشكر والتقدير..")
                .FontSize(14);

            // QR + signature
            col.Item().PaddingTop(15);
            col.Item().Element(c => ComposeFooterSection(c, data));
        });
    }

    private static void AddTableRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(5)
            .Text(label).FontSize(14).Bold();
        table.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(5)
            .Text(value).FontSize(14);
    }

    private void ComposeFooterSection(IContainer container, DocumentGenerationData data)
    {
        container.Column(col =>
        {
            // Signature area
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(sigCol =>
                {
                    sigCol.Item().Text("الاسم / Name")
                        .FontSize(10).Bold();
                    sigCol.Item().PaddingTop(18).Width(180).LineHorizontal(0.5f).LineColor(Colors.Grey.Darken1);

                    sigCol.Item().PaddingTop(12).Text("التوقيع / Signature")
                        .FontSize(10).Bold();
                    sigCol.Item().PaddingTop(18).Width(180).LineHorizontal(0.5f).LineColor(Colors.Grey.Darken1);
                });

                row.AutoItem().AlignLeft().Width(130).Column(stampCol =>
                {
                    stampCol.Item().Text("الختم / Stamp")
                        .FontSize(10).Bold();
                    stampCol.Item().PaddingTop(4)
                        .Height(55).Width(120)
                        .Border(0.5f).BorderColor(Colors.Grey.Lighten1);
                });
            });

            // QR & issued-by info
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

                row.AutoItem().AlignLeft().Width(65).Height(65)
                    .Image(data.QrCodeImageBytes);
            });
        });
    }
}
