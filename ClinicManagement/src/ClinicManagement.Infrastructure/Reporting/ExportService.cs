using System.Reflection;
using ClosedXML.Excel;
using ClinicManagement.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ClinicManagement.Infrastructure.Reporting;

/// <summary>
/// Column headers and cell values are derived from T's public properties
/// via reflection, so every report DTO in Features/Reports/DTOs gets both
/// export formats for free — no per-report export code. Property names are
/// split on PascalCase boundaries for headers ("PatientCode" → "Patient
/// Code").
/// </summary>
public class ExportService : IExportService
{
    static ExportService()
    {
        // QuestPDF's Community license is free for small businesses/individuals (revenue/funding
        // thresholds apply — see https://www.questpdf.com/license/) — set explicitly here since
        // QuestPDF throws at render time if this isn't configured. Swap to LicenseType.Commercial
        // if the deploying organization doesn't qualify for Community terms.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportToExcel<T>(IReadOnlyList<T> rows, string sheetName)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(SanitizeSheetName(sheetName));

        for (var col = 0; col < properties.Length; col++)
        {
            var cell = worksheet.Cell(1, col + 1);
            cell.Value = ToHeaderText(properties[col].Name);
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        for (var row = 0; row < rows.Count; row++)
        {
            for (var col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(rows[row]);
                worksheet.Cell(row + 2, col + 1).Value = FormatCellValue(value);
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportToPdf<T>(string title, IReadOnlyList<T> rows)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Text(title).FontSize(16).Bold();

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in properties) columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        foreach (var property in properties)
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4)
                                .Text(ToHeaderText(property.Name)).Bold();
                        }
                    });

                    foreach (var row in rows)
                    {
                        foreach (var property in properties)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(FormatCellValue(property.GetValue(row)));
                        }
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated ").FontSize(8);
                    text.Span(DateTime.UtcNow.ToString("g")).FontSize(8);
                    text.Span(" UTC").FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string FormatCellValue(object? value) => value switch
    {
        null => string.Empty,
        DateTime dt => dt.ToString("g"),
        decimal dec => dec.ToString("N2"),
        _ => value.ToString() ?? string.Empty
    };

    /// <summary>"TotalAppointments" -> "Total Appointments".</summary>
    private static string ToHeaderText(string propertyName) =>
        string.Concat(propertyName.Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));

    private static string SanitizeSheetName(string name)
    {
        // Excel sheet names: max 31 chars, no \ / ? * [ ]
        var invalidChars = new[] { '\\', '/', '?', '*', '[', ']' };
        var cleaned = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
        return cleaned.Length > 31 ? cleaned[..31] : cleaned;
    }
}
