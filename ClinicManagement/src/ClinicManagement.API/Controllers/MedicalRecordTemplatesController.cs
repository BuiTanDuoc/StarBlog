using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.MedicalRecordTemplates.Commands.CreateMedicalRecordTemplate;
using ClinicManagement.Application.Features.MedicalRecordTemplates.Commands.DeleteMedicalRecordTemplate;
using ClinicManagement.Application.Features.MedicalRecordTemplates.Commands.UpdateMedicalRecordTemplate;
using ClinicManagement.Application.Features.MedicalRecordTemplates.Queries.GetMedicalRecordTemplatesList;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

/// <summary>
/// Specialty exam templates (Dental Exam, Eye Exam, ...) that a doctor can
/// pick from when creating a MedicalRecord — see MedicalRecordsController
/// for the record itself. Listing is available to anyone who can view
/// medical records (a doctor needs the list to pick a template); managing
/// (create/update/delete) is a clinic-configuration action.
/// </summary>
public class MedicalRecordTemplatesController : ApiControllerBase
{
    [HasPermission(Permissions.MedicalRecords.View)]
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GetMedicalRecordTemplatesListQuery query) =>
        Ok(await Mediator.Send(query));

    [HasPermission(Permissions.MedicalRecords.ManageTemplates)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateMedicalRecordTemplateCommand command) =>
        HandleResult(await Mediator.Send(command));

    [HasPermission(Permissions.MedicalRecords.ManageTemplates)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMedicalRecordTemplateCommand command)
    {
        if (id != command.Id) return BadRequest(new { succeeded = false, errors = new[] { "Route id does not match body id." } });
        return HandleResult(await Mediator.Send(command));
    }

    [HasPermission(Permissions.MedicalRecords.ManageTemplates)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) => HandleResult(await Mediator.Send(new DeleteMedicalRecordTemplateCommand(id)));
}
