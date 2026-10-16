using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Expenses.Commands.CreateExpense;
using ClinicManagement.Application.Features.Expenses.Commands.DeleteExpense;
using ClinicManagement.Application.Features.Expenses.Commands.UpdateExpense;
using ClinicManagement.Application.Features.Expenses.Queries.GetExpensesList;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[HasPermission(Permissions.Billing.ManageExpenses)]
public class ExpensesController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GetExpensesListQuery query) =>
        Ok(await Mediator.Send(query));

    [HttpPost]
    public async Task<IActionResult> Create(CreateExpenseCommand command) =>
        HandleResult(await Mediator.Send(command));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateExpenseCommand command)
    {
        if (id != command.Id) return BadRequest(new { succeeded = false, errors = new[] { "Route id does not match body id." } });
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        HandleResult(await Mediator.Send(new DeleteExpenseCommand(id)));
}
