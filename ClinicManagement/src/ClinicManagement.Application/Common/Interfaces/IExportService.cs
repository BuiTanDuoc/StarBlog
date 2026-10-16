namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// Generic tabular export — works off any row DTO via reflection over its
/// public properties, so a new report type never needs new export code,
/// just a new query that produces a flat DTO. Implemented in Infrastructure
/// using ClosedXML (Excel) and QuestPDF (PDF).
/// </summary>
public interface IExportService
{
    byte[] ExportToExcel<T>(IReadOnlyList<T> rows, string sheetName);

    byte[] ExportToPdf<T>(string title, IReadOnlyList<T> rows);
}
