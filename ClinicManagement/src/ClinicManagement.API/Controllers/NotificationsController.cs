using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using ClinicManagement.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using ClinicManagement.Application.Features.Notifications.Queries.GetMyNotifications;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

/// <summary>Always scoped to the caller — there is no "get notifications for another user"
/// endpoint, by design (see MarkNotificationAsReadCommandHandler's ownership check).</summary>
public class NotificationsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] GetMyNotificationsQuery query) =>
        Ok(await Mediator.Send(query));

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id) =>
        HandleResult(await Mediator.Send(new MarkNotificationAsReadCommand(id)));

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead() =>
        HandleResult(await Mediator.Send(new MarkAllNotificationsAsReadCommand()));
}
