using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;
using ClinicManagement.Application.Features.MedicalRecords.Commands.UpdateMedicalRecord;
using ClinicManagement.Application.Features.MedicalRecords.Queries.GetMedicalRecordById;
using ClinicManagement.Application.Features.MedicalRecords.Queries.GetMedicalRecordsByPatient;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class MedicalRecordsController : ApiControllerBase
{
    [HasPermission(Permissions.MedicalRecords.View)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        HandleResult(await Mediator.Send(new GetMedicalRecordByIdQuery(id)));

    [HasPermission(Permissions.MedicalRecords.View)]
    [HttpGet("by-patient/{patientId:guid}")]
    public async Task<IActionResult> GetByPatient(Guid patientId) =>
        Ok(await Mediator.Send(new GetMedicalRecordsByPatientQuery(patientId)));

    [HasPermission(Permissions.MedicalRecords.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateMedicalRecordCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
    }

    [HasPermission(Permissions.MedicalRecords.Edit)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMedicalRecordCommand command)
    {
        if (id != command.Id) return BadRequest(new { succeeded = false, errors = new[] { "Route id does not match body id." } });
        return HandleResult(await Mediator.Send(command));
    }
}
