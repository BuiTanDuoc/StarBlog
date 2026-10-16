using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Appointments.Commands.CancelAppointment;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.PatientPortal.Commands.CancelOwnAppointment;

/// <summary>
/// CancelAppointmentCommand itself has no ownership check — it's used by
/// staff who are allowed to cancel any patient's appointment. This wrapper
/// adds the one check that matters for the portal: the appointment being
/// cancelled must belong to the caller's own PatientId, verified against
/// ICurrentUserService.PatientId (server-derived from the JWT), never a
/// client-supplied patient id.
/// </summary>
public record CancelOwnAppointmentCommand(Guid AppointmentId, string Reason) : IRequest<Result>;

public class CancelOwnAppointmentCommandValidator : AbstractValidator<CancelOwnAppointmentCommand>
{
    public CancelOwnAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class CancelOwnAppointmentCommandHandler : IRequestHandler<CancelOwnAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISender _sender;

    public CancelOwnAppointmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ISender sender)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _sender = sender;
    }

    public async Task<Result> Handle(CancelOwnAppointmentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.PatientId is not { } patientId)
        {
            return Result.Failure("This account is not linked to a patient record.");
        }

        var ownerPatientId = await _unitOfWork.Repository<Appointment>().Query()
            .Where(a => a.Id == request.AppointmentId)
            .Select(a => (Guid?)a.PatientId)
            .FirstOrDefaultAsync(cancellationToken);

        if (ownerPatientId is null)
        {
            return Result.Failure("Appointment not found.");
        }

        if (ownerPatientId != patientId)
        {
            // Same message as "not found" on purpose — confirming an appointment ID belongs
            // to someone else is itself a (small) information leak.
            return Result.Failure("Appointment not found.");
        }

        return await _sender.Send(new CancelAppointmentCommand(request.AppointmentId, request.Reason), cancellationToken);
    }
}
