using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Plans.Commands.CreatePlan;
using ClinicManagement.Application.Features.Plans.Commands.UpdatePlan;
using ClinicManagement.Application.Features.Plans.Queries.GetPlansList;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class PlansController : ApiControllerBase
{
    /// <summary>Public pricing page data — no auth required.</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] bool activeOnly = true) =>
        Ok(await Mediator.Send(new GetPlansListQuery(activeOnly)));

    [HasPermission(Permissions.Tenants.Manage)]
    [HttpPost]
    public async Task<IActionResult> Create(CreatePlanCommand command) =>
        HandleResult(await Mediator.Send(command));

    [HasPermission(Permissions.Tenants.Manage)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePlanCommand command)
    {
        if (id != command.Id) return BadRequest(new { succeeded = false, errors = new[] { "Route id does not match body id." } });
        return HandleResult(await Mediator.Send(command));
    }
}
