using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Appointments.Commands.CancelAppointment;
using ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicManagement.Application.Features.Appointments.Commands.RescheduleAppointment;
using ClinicManagement.Application.Features.Appointments.Commands.UpdateAppointmentStatus;
using ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentById;
using ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentTimeline;
using ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentsList;
using ClinicManagement.Application.Features.Appointments.Queries.GetDoctorAvailability;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class AppointmentsController : ApiControllerBase
{
    [HasPermission(Permissions.Appointments.View)]
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GetAppointmentsListQuery query) =>
        Ok(await Mediator.Send(query));

    [HasPermission(Permissions.Appointments.View)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        HandleResult(await Mediator.Send(new GetAppointmentByIdQuery(id)));

    [HasPermission(Permissions.Appointments.View)]
    [HttpGet("{id:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid id) =>
        Ok(await Mediator.Send(new GetAppointmentTimelineQuery(id)));

    /// <summary>Free/busy slots for a doctor on a given date — powers the booking calendar UI.</summary>
    [HasPermission(Permissions.Appointments.View)]
    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability([FromQuery] GetDoctorAvailabilityQuery query) =>
        Ok(await Mediator.Send(query));

    [HasPermission(Permissions.Appointments.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAppointmentCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
    }

    [HasPermission(Permissions.Appointments.Edit)]
    [HttpPut("{id:guid}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleBody body) =>
        HandleResult(await Mediator.Send(new RescheduleAppointmentCommand(id, body.NewStart, body.NewEnd)));

    [HasPermission(Permissions.Appointments.Edit)]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusBody body) =>
        HandleResult(await Mediator.Send(new UpdateAppointmentStatusCommand(id, body.NewStatus, body.Reason)));

    [HasPermission(Permissions.Appointments.Cancel)]
    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBody body) =>
        HandleResult(await Mediator.Send(new CancelAppointmentCommand(id, body.Reason)));

    public record RescheduleBody(DateTime NewStart, DateTime NewEnd);
    public record UpdateStatusBody(AppointmentStatus NewStatus, string? Reason);
    public record CancelBody(string Reason);
}
