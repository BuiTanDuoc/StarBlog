using ClinicManagement.API.Common;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Reports.Queries.GetAppointmentsReport;
using ClinicManagement.Application.Features.Reports.Queries.GetDoctorPerformanceReport;
using ClinicManagement.Application.Features.Reports.Queries.GetPatientsReport;
using ClinicManagement.Application.Features.Reports.Queries.GetRevenueReport;
using ClinicManagement.Application.Features.Reports.Queries.GetSubscriptionReport;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

/// <summary>?format=pdf|excel on every endpoint; defaults to excel. Reports are computed
/// fresh on every request (no caching) — acceptable given typical clinic data volumes and
/// the low frequency reports are pulled at; revisit with output caching if that changes.</summary>
public class ReportsController : ApiControllerBase
{
    private readonly IExportService _exportService;

    public ReportsController(IExportService exportService)
    {
        _exportService = exportService;
    }

    [HasPermission(Permissions.Reports.View)]
    [HttpGet("patients")]
    public async Task<IActionResult> Patients(DateTime? from, DateTime? to, string format = "excel")
    {
        var rows = await Mediator.Send(new GetPatientsReportQuery(from, to));
        return Export(rows, "Patients Report", format);
    }

    [HasPermission(Permissions.Reports.View)]
    [HttpGet("appointments")]
    public async Task<IActionResult> Appointments(DateTime? from, DateTime? to, AppointmentStatus? status, string format = "excel")
    {
        var rows = await Mediator.Send(new GetAppointmentsReportQuery(from, to, status));
        return Export(rows, "Appointments Report", format);
    }

    [HasPermission(Permissions.Reports.View)]
    [HttpGet("revenue")]
    public async Task<IActionResult> Revenue(DateTime? from, DateTime? to, string format = "excel")
    {
        var rows = await Mediator.Send(new GetRevenueReportQuery(from, to));
        return Export(rows, "Revenue Report", format);
    }

    [HasPermission(Permissions.Reports.View)]
    [HttpGet("doctor-performance")]
    public async Task<IActionResult> DoctorPerformance(DateTime? from, DateTime? to, string format = "excel")
    {
        var rows = await Mediator.Send(new GetDoctorPerformanceReportQuery(from, to));
        return Export(rows, "Doctor Performance Report", format);
    }

    /// <summary>Cross-tenant — SuperAdmin only (enforced inside the handler, not just here).</summary>
    [HttpGet("subscriptions")]
    public async Task<IActionResult> Subscriptions(string format = "excel")
    {
        var result = await Mediator.Send(new GetSubscriptionReportQuery());
        return result.Succeeded
            ? Export(result.Data!, "Subscription Report", format)
            : BadRequest(new { succeeded = false, errors = result.Errors });
    }

    private FileContentResult Export<T>(IReadOnlyList<T> rows, string title, string format)
    {
        var isPdf = string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase);
        var fileName = $"{title.Replace(" ", "-")}-{DateTime.UtcNow:yyyyMMdd}";

        return isPdf
            ? File(_exportService.ExportToPdf(title, rows), "application/pdf", $"{fileName}.pdf")
            : File(_exportService.ExportToExcel(rows, title), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }
}
