using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.PatientPackages.Commands.SellPackageToPatient;
using ClinicManagement.Application.Features.PatientPackages.Queries.GetPatientPackagesList;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class PatientPackagesController : ApiControllerBase
{
    [HasPermission(Permissions.Billing.SellTreatmentPackages)]
    [HttpGet]
    public async Task<IActionResult> GetByPatient([FromQuery] Guid patientId) =>
        Ok(await Mediator.Send(new GetPatientPackagesListQuery(patientId)));

    [HasPermission(Permissions.Billing.SellTreatmentPackages)]
    [HttpPost]
    public async Task<IActionResult> Sell(SellPackageToPatientCommand command) =>
        HandleResult(await Mediator.Send(command));
}
