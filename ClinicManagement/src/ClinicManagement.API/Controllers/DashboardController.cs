using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Dashboard.Queries.GetClinicOwnerDashboard;
using ClinicManagement.Application.Features.Dashboard.Queries.GetDoctorDashboard;
using ClinicManagement.Application.Features.Dashboard.Queries.GetReceptionistDashboard;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class DashboardController : ApiControllerBase
{
    [Authorize(Roles = Roles.ClinicOwner)]
    [HttpGet("clinic-owner")]
    public async Task<IActionResult> ClinicOwner() =>
        Ok(await Mediator.Send(new GetClinicOwnerDashboardQuery()));

    [Authorize(Roles = Roles.Doctor)]
    [HttpGet("doctor")]
    public async Task<IActionResult> Doctor() =>
        Ok(await Mediator.Send(new GetDoctorDashboardQuery()));

    [Authorize(Roles = $"{Roles.Receptionist},{Roles.ClinicOwner}")]
    [HttpGet("receptionist")]
    public async Task<IActionResult> Receptionist() =>
        Ok(await Mediator.Send(new GetReceptionistDashboardQuery()));
}
