using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.AuditLogs.Queries.GetAuditLogsList;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Authorize(Roles = $"{Roles.ClinicOwner},{Roles.SuperAdmin}")]
public class AuditLogsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GetAuditLogsListQuery query) =>
        Ok(await Mediator.Send(query));
}
