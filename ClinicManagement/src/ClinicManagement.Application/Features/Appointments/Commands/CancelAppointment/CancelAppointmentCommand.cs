using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Appointments.Events;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand(Guid Id, string Reason) : IRequest<Result>;

public class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublisher _publisher;

    public CancelAppointmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _publisher = publisher;
    }

    public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Appointment>();
        var appointment = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (appointment is null)
        {
            return Result.Failure($"Appointment with id '{request.Id}' was not found.");
        }

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
        {
            return Result.Failure($"An appointment that is already '{appointment.Status}' cannot be cancelled.");
        }

        var previousStatus = appointment.Status;
        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = request.Reason;

        if (appointment.PatientPackageId is { } packageId)
        {
            var patientPackage = await _unitOfWork.Repository<PatientPackage>().GetByIdAsync(packageId, cancellationToken);
            if (patientPackage is not null && patientPackage.SessionsUsed > 0)
            {
                // Give the session back — the patient never actually attended, so it
                // shouldn't count against their prepaid balance. Guarded with > 0 so this
                // can't push SessionsUsed negative if something upstream is already
                // inconsistent (defensive, not expected to trigger in normal use).
                patientPackage.SessionsUsed -= 1;
                _unitOfWork.Repository<PatientPackage>().Update(patientPackage);
            }
        }

        appointment.StatusHistory.Add(new AppointmentStatusHistory
        {
            TenantId = _currentUserService.TenantId,
            AppointmentId = appointment.Id,
            FromStatus = previousStatus,
            ToStatus = AppointmentStatus.Cancelled,
            ChangedBy = _currentUserService.UserName,
            Reason = request.Reason
        });

        repository.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new AppointmentCancelledEvent(appointment.Id, appointment.DoctorId, appointment.PatientId, request.Reason),
            cancellationToken);

        return Result.Success();
    }
}
