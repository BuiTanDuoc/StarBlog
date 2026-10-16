using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Patients.Commands.CreatePatient;
using ClinicManagement.Application.Features.Patients.Commands.DeletePatient;
using ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;
using ClinicManagement.Application.Features.Patients.Queries.GetPatientById;
using ClinicManagement.Application.Features.Patients.Queries.GetPatientsList;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class PatientsController : ApiControllerBase
{
    /// <summary>Search + sort + paginate. SearchTerm matches name, patient code, phone, or national ID.</summary>
    [HasPermission(Permissions.Patients.View)]
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GetPatientsListQuery query) =>
        Ok(await Mediator.Send(query));

    [HasPermission(Permissions.Patients.View)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        HandleResult(await Mediator.Send(new GetPatientByIdQuery(id)));

    [HasPermission(Permissions.Patients.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(CreatePatientCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
    }

    [HasPermission(Permissions.Patients.Edit)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePatientCommand command)
    {
        if (id != command.Id) return BadRequest(new { succeeded = false, errors = new[] { "Route id does not match body id." } });
        return HandleResult(await Mediator.Send(command));
    }

    [HasPermission(Permissions.Patients.Delete)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        HandleResult(await Mediator.Send(new DeletePatientCommand(id)));
}
