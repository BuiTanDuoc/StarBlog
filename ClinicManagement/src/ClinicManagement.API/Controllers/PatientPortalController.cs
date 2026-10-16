using ClinicManagement.API.Common;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentsList;
using ClinicManagement.Application.Features.Appointments.Queries.GetDoctorAvailability;
using ClinicManagement.Application.Features.Doctors.Queries.GetDoctorsList;
using ClinicManagement.Application.Features.Invoices.Commands.CreateInvoiceCheckout;
using ClinicManagement.Application.Features.Invoices.Queries.GetInvoicesList;
using ClinicManagement.Application.Features.MedicalRecords.Queries.GetMedicalRecordsByPatient;
using ClinicManagement.Application.Features.PatientPackages.Queries.GetPatientPackagesList;
using ClinicManagement.Application.Features.PatientPortal.Commands.BookOwnAppointment;
using ClinicManagement.Application.Features.PatientPortal.Commands.CancelOwnAppointment;
using ClinicManagement.Application.Features.PatientPortal.Commands.RegisterPatientAccount;
using ClinicManagement.Application.Features.PatientPortal.Queries.GetMyProfile;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Controllers;

/// <summary>
/// Every non-anonymous action here forces PatientId from
/// ICurrentUserService.PatientId (a claim baked into the JWT at login —
/// see TokenService/AuthTokenIssuer) rather than accepting it from the
/// client. This is the entire security boundary of the patient portal:
/// a client-supplied patientId anywhere in this controller would let one
/// patient read or modify another patient's medical/financial records.
/// </summary>
[Route("api/patient-portal")]
public class PatientPortalController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public PatientPortalController(ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Public self-signup — picks the clinic by its Slug (e.g. from the "/portal/{slug}" URL).</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterPatientAccountCommand command)
    {
        var result = await Mediator.Send(command with { IpAddress = ClientIpAddress });
        return HandleResult(result);
    }

    [HasPermission(Permissions.Portal.ViewOwnProfile)]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile() => HandleResult(await Mediator.Send(new GetMyProfileQuery()));

    [HasPermission(Permissions.Portal.ViewOwnAppointments)]
    [HttpGet("appointments")]
    public async Task<IActionResult> GetMyAppointments([FromQuery] GetAppointmentsListQuery query) =>
        Ok(await Mediator.Send(query with { PatientId = RequirePatientId() }));

    [HasPermission(Permissions.Portal.CreateOwnAppointments)]
    [HttpPost("appointments")]
    public async Task<IActionResult> BookAppointment(BookOwnAppointmentCommand command) =>
        HandleResult(await Mediator.Send(command));

    [HasPermission(Permissions.Portal.CancelOwnAppointments)]
    [HttpPut("appointments/{id:guid}/cancel")]
    public async Task<IActionResult> CancelAppointment(Guid id, CancelAppointmentReasonRequest request) =>
        HandleResult(await Mediator.Send(new CancelOwnAppointmentCommand(id, request.Reason)));

    /// <summary>Free/busy slots only (start/end times, no patient identities) — safe to expose to any authenticated patient regardless of which doctor they ask about, same as the public booking flow needs.</summary>
    [HasPermission(Permissions.Portal.CreateOwnAppointments)]
    [HttpGet("doctors/{doctorId:guid}/availability")]
    public async Task<IActionResult> GetDoctorAvailability(Guid doctorId, [FromQuery] DateOnly date, [FromQuery] int slotMinutes = 30) =>
        Ok(await Mediator.Send(new GetDoctorAvailabilityQuery(doctorId, date, slotMinutes)));

    /// <summary>Doctor picker for the booking flow. Reuses the staff GetDoctorsListQuery/DoctorListItemDto — that shape carries name/specialization/branch, nothing patients shouldn't see.</summary>
    [HasPermission(Permissions.Portal.CreateOwnAppointments)]
    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors([FromQuery] GetDoctorsListQuery query) => Ok(await Mediator.Send(query));

    [HasPermission(Permissions.Portal.ViewOwnMedicalRecords)]
    [HttpGet("medical-records")]
    public async Task<IActionResult> GetMyMedicalRecords() =>
        Ok(await Mediator.Send(new GetMedicalRecordsByPatientQuery(RequirePatientId())));

    [HasPermission(Permissions.Portal.ViewOwnInvoices)]
    [HttpGet("invoices")]
    public async Task<IActionResult> GetMyInvoices([FromQuery] GetInvoicesListQuery query) =>
        Ok(await Mediator.Send(query with { PatientId = RequirePatientId() }));

    /// <summary>Used by the booking flow to offer "redeem a session from my package" instead of a fresh invoice — see BookOwnAppointmentCommand's PatientPackageId.</summary>
    [HasPermission(Permissions.Portal.CreateOwnAppointments)]
    [HttpGet("packages")]
    public async Task<IActionResult> GetMyPackages() =>
        Ok(await Mediator.Send(new GetPatientPackagesListQuery(RequirePatientId())));

    /// <summary>
    /// Same checkout-session mechanics as the staff endpoint (InvoicesController), but with
    /// an ownership check first — a patient may only start a checkout for their own invoice,
    /// verified against ICurrentUserService.PatientId, never a value the client could forge.
    /// </summary>
    [HasPermission(Permissions.Portal.PayOwnInvoices)]
    [HttpPost("invoices/{id:guid}/checkout")]
    public async Task<IActionResult> PayInvoice(Guid id, CreatePatientPortalCheckoutBody body)
    {
        var patientId = RequirePatientId();
        var ownerPatientId = await _unitOfWork.Repository<Invoice>().Query()
            .Where(i => i.Id == id)
            .Select(i => (Guid?)i.PatientId)
            .FirstOrDefaultAsync();

        if (ownerPatientId != patientId)
        {
            // Same "not found" for both missing and someone-else's invoice — see the identical
            // reasoning in CancelOwnAppointmentCommand.
            return NotFound();
        }

        return HandleResult(await Mediator.Send(new CreateInvoiceCheckoutCommand(id, body.SuccessUrl, body.CancelUrl)));
    }

    /// <summary>
    /// Throws (caught by the global exception middleware as a 500) rather than silently
    /// returning Guid.Empty if a token somehow reaches here without a patient_id claim —
    /// RolePermissions.Map never grants Portal.* to a non-Patient role, so this should be
    /// unreachable in practice, but "unreachable" is not the same guarantee as "checked".
    /// </summary>
    private Guid RequirePatientId() =>
        _currentUserService.PatientId ?? throw new InvalidOperationException("Authenticated portal request without a patient_id claim.");
}

public record CancelAppointmentReasonRequest(string Reason);
public record CreatePatientPortalCheckoutBody(string SuccessUrl, string CancelUrl);
