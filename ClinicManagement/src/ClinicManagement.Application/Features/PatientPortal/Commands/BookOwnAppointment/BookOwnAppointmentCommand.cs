using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicManagement.Application.Features.Appointments.DTOs;
using ClinicManagement.Domain.Common;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.PatientPortal.Commands.BookOwnAppointment;

/// <summary>
/// Patient-portal booking. Deliberately thin: it forwards to the same
/// CreateAppointmentCommand staff use (so subscription limits, working-hours
/// and double-booking checks, and the doctor notification all stay in one
/// place), but forces PatientId from the caller's own JWT (ICurrentUserService.PatientId)
/// instead of trusting a client-supplied value — the entire point of this
/// command existing separately from CreateAppointmentCommand is that a
/// patient must never be able to book (or see) an appointment for anyone
/// but themselves.
/// </summary>
public record BookOwnAppointmentCommand : IRequest<Result<AppointmentDto>>
{
    public Guid DoctorId { get; init; }
    public Guid BranchId { get; init; }
    public DateTime ScheduledStart { get; init; }
    public DateTime ScheduledEnd { get; init; }
    public string? Notes { get; init; }

    /// <summary>Optional — redeem a session from one of the patient's own prepaid packages instead of a fresh invoice. Ownership is (re)checked in CreateAppointmentCommand against the JWT-derived PatientId below, not trusted from this value alone.</summary>
    public Guid? PatientPackageId { get; init; }
}

public class BookOwnAppointmentCommandValidator : AbstractValidator<BookOwnAppointmentCommand>
{
    public BookOwnAppointmentCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.ScheduledStart).NotEmpty().GreaterThan(DateTime.UtcNow).WithMessage("Cannot book an appointment in the past.");
        RuleFor(x => x.ScheduledEnd).GreaterThan(x => x.ScheduledStart);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public class BookOwnAppointmentCommandHandler : IRequestHandler<BookOwnAppointmentCommand, Result<AppointmentDto>>
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUserService;

    public BookOwnAppointmentCommandHandler(ISender sender, ICurrentUserService currentUserService)
    {
        _sender = sender;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AppointmentDto>> Handle(BookOwnAppointmentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.PatientId is not { } patientId)
        {
            // Only reachable if a non-patient token somehow carries the Portal.CreateOwnAppointments
            // permission, which RolePermissions.Map never grants — defense in depth, not the primary guard.
            return Result<AppointmentDto>.Failure("This account is not linked to a patient record.");
        }

        return await _sender.Send(new CreateAppointmentCommand
        {
            PatientId = patientId,
            DoctorId = request.DoctorId,
            BranchId = request.BranchId,
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledEnd,
            Notes = request.Notes,
            PatientPackageId = request.PatientPackageId
        }, cancellationToken);
    }
}
