using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Employees.Commands.CreateEmployee;
using ClinicManagement.Application.Features.Employees.Commands.DeleteEmployee;
using ClinicManagement.Application.Features.Employees.Commands.UpdateEmployee;
using ClinicManagement.Application.Features.Employees.Queries.GetEmployeeById;
using ClinicManagement.Application.Features.Employees.Queries.GetEmployeesList;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Authorize(Roles = Roles.ClinicOwner)]
public class EmployeesController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GetEmployeesListQuery query) =>
        Ok(await Mediator.Send(query));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        HandleResult(await Mediator.Send(new GetEmployeeByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateEmployeeCommand command)
    {
        if (id != command.Id) return BadRequest(new { succeeded = false, errors = new[] { "Route id does not match body id." } });
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        HandleResult(await Mediator.Send(new DeleteEmployeeCommand(id)));
}
