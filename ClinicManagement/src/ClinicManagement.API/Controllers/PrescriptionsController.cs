using ClinicManagement.API.Common;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Prescriptions.Commands.CreatePrescription;
using ClinicManagement.Application.Features.Prescriptions.Queries.GetPrescriptionById;
using ClinicManagement.Application.Features.Prescriptions.Queries.GetPrescriptionsByPatient;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class PrescriptionsController : ApiControllerBase
{
    private readonly IExportService _exportService;

    public PrescriptionsController(IExportService exportService)
    {
        _exportService = exportService;
    }

    [HasPermission(Permissions.MedicalRecords.View)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        HandleResult(await Mediator.Send(new GetPrescriptionByIdQuery(id)));

    [HasPermission(Permissions.MedicalRecords.View)]
    [HttpGet("by-patient/{patientId:guid}")]
    public async Task<IActionResult> GetByPatient(Guid patientId) =>
        Ok(await Mediator.Send(new GetPrescriptionsByPatientQuery(patientId)));

    [HasPermission(Permissions.MedicalRecords.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(CreatePrescriptionCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
    }

    /// <summary>
    /// Renders the prescription's medicine list as a PDF via the same generic
    /// IExportService used by the reporting module — a simple itemized table
    /// rather than a fully styled prescription pad, which would need its own
    /// bespoke QuestPDF layout (clinic letterhead, doctor signature block, etc.).
    /// </summary>
    [HasPermission(Permissions.MedicalRecords.View)]
    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid id)
    {
        var result = await Mediator.Send(new GetPrescriptionByIdQuery(id));
        if (!result.Succeeded || result.Data is null)
        {
            return BadRequest(new { succeeded = false, errors = result.Errors });
        }

        var title = $"Prescription — {result.Data.PatientName} — {result.Data.DateIssued:d}";
        var pdfBytes = _exportService.ExportToPdf(title, result.Data.Items);

        return File(pdfBytes, "application/pdf", $"Prescription-{id}.pdf");
    }
}
