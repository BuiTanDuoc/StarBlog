using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Departments.Commands.CreateDepartment;
using ClinicManagement.Application.Features.Departments.Commands.DeleteDepartment;
using ClinicManagement.Application.Features.Departments.Commands.UpdateDepartment;
using ClinicManagement.Application.Features.Departments.Queries.GetDepartmentsList;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class DepartmentsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] Guid? branchId) =>
        Ok(await Mediator.Send(new GetDepartmentsListQuery(branchId)));

    [Authorize(Roles = Roles.ClinicOwner)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateDepartmentCommand command) =>
        HandleResult(await Mediator.Send(command));

    [Authorize(Roles = Roles.ClinicOwner)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateDepartmentCommand command)
    {
        if (id != command.Id) return BadRequest(new { succeeded = false, errors = new[] { "Route id does not match body id." } });
        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Roles = Roles.ClinicOwner)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        HandleResult(await Mediator.Send(new DeleteDepartmentCommand(id)));
}
