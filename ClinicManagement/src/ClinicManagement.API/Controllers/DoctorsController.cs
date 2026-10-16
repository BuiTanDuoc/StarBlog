using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Doctors.Commands.CreateDoctor;
using ClinicManagement.Application.Features.Doctors.Commands.DeleteDoctor;
using ClinicManagement.Application.Features.Doctors.Commands.UpdateDoctor;
using ClinicManagement.Application.Features.Doctors.Queries.GetDoctorById;
using ClinicManagement.Application.Features.Doctors.Queries.GetDoctorsList;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class DoctorsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GetDoctorsListQuery query) =>
        Ok(await Mediator.Send(query));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        HandleResult(await Mediator.Send(new GetDoctorByIdQuery(id)));

    [Authorize(Roles = Roles.ClinicOwner)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateDoctorCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
    }

    [Authorize(Roles = Roles.ClinicOwner)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateDoctorCommand command)
    {
        if (id != command.Id) return BadRequest(new { succeeded = false, errors = new[] { "Route id does not match body id." } });
        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Roles = Roles.ClinicOwner)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        HandleResult(await Mediator.Send(new DeleteDoctorCommand(id)));
}
