using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.TreatmentPackages.Commands.CreateTreatmentPackage;
using ClinicManagement.Application.Features.TreatmentPackages.Commands.DeleteTreatmentPackage;
using ClinicManagement.Application.Features.TreatmentPackages.Commands.UpdateTreatmentPackage;
using ClinicManagement.Application.Features.TreatmentPackages.Queries.GetTreatmentPackagesList;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

/// <summary>
/// The sellable catalog (see TreatmentPackage) — listing is available to
/// anyone who can sell one (a receptionist needs the list at checkout);
/// managing the catalog itself is a clinic-configuration action. Selling a
/// package to a specific patient is PatientPackagesController, not here.
/// </summary>
public class TreatmentPackagesController : ApiControllerBase
{
    [HasPermission(Permissions.Billing.SellTreatmentPackages)]
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GetTreatmentPackagesListQuery query) =>
        Ok(await Mediator.Send(query));

    [HasPermission(Permissions.Billing.ManageTreatmentPackages)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateTreatmentPackageCommand command) =>
        HandleResult(await Mediator.Send(command));

    [HasPermission(Permissions.Billing.ManageTreatmentPackages)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTreatmentPackageCommand command)
    {
        if (id != command.Id) return BadRequest(new { succeeded = false, errors = new[] { "Route id does not match body id." } });
        return HandleResult(await Mediator.Send(command));
    }

    [HasPermission(Permissions.Billing.ManageTreatmentPackages)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) => HandleResult(await Mediator.Send(new DeleteTreatmentPackageCommand(id)));
}
