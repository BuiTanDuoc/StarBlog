using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Branches.Commands.CreateBranch;
using ClinicManagement.Application.Features.Branches.Commands.DeleteBranch;
using ClinicManagement.Application.Features.Branches.Commands.UpdateBranch;
using ClinicManagement.Application.Features.Branches.Queries.GetBranchById;
using ClinicManagement.Application.Features.Branches.Queries.GetBranchesList;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

/// <summary>Branch setup is clinic-administrative, not tied to a fine-grained Permission —
/// gated by role (any staff can view, only ClinicOwner mutates) rather than adding a new
/// Permissions.Branches.* family for a low-frequency admin task.</summary>
public class BranchesController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] bool activeOnly = false) =>
        Ok(await Mediator.Send(new GetBranchesListQuery(activeOnly)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        HandleResult(await Mediator.Send(new GetBranchByIdQuery(id)));

    [Authorize(Roles = Roles.ClinicOwner)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateBranchCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
    }

    [Authorize(Roles = Roles.ClinicOwner)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateBranchCommand command)
    {
        if (id != command.Id) return BadRequest(new { succeeded = false, errors = new[] { "Route id does not match body id." } });
        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Roles = Roles.ClinicOwner)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        HandleResult(await Mediator.Send(new DeleteBranchCommand(id)));
}
