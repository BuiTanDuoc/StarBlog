using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Appointments.Events;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.RescheduleAppointment;

public record RescheduleAppointmentCommand(Guid Id, DateTime NewStart, DateTime NewEnd) : IRequest<Result>;

public class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NewStart).GreaterThan(DateTime.UtcNow).WithMessage("Cannot reschedule to a time in the past.");
        RuleFor(x => x.NewEnd).GreaterThan(x => x.NewStart);
    }
}

public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAppointmentAvailabilityService _availabilityService;
    private readonly IPublisher _publisher;

    public RescheduleAppointmentCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IAppointmentAvailabilityService availabilityService,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _availabilityService = availabilityService;
        _publisher = publisher;
    }

    public async Task<Result> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Appointment>();
        var appointment = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (appointment is null)
        {
            return Result.Failure($"Appointment with id '{request.Id}' was not found.");
        }

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
        {
            return Result.Failure($"An appointment with status '{appointment.Status}' cannot be rescheduled.");
        }

        await _availabilityService.EnsureSlotIsAvailableAsync(
            appointment.DoctorId, request.NewStart, request.NewEnd, excludeAppointmentId: appointment.Id,
            cancellationToken: cancellationToken);

        var previousStatus = appointment.Status;
        var oldStart = appointment.ScheduledStart;
        appointment.ScheduledStart = request.NewStart;
        appointment.ScheduledEnd = request.NewEnd;
        appointment.Status = AppointmentStatus.Pending; // re-confirmation required after a reschedule

        appointment.StatusHistory.Add(new AppointmentStatusHistory
        {
            TenantId = _currentUserService.TenantId,
            AppointmentId = appointment.Id,
            FromStatus = previousStatus,
            ToStatus = AppointmentStatus.Pending,
            ChangedBy = _currentUserService.UserName,
            Reason = $"Rescheduled from {oldStart:g} to {request.NewStart:g}"
        });

        repository.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new AppointmentRescheduledEvent(appointment.Id, appointment.DoctorId, appointment.PatientId, request.NewStart),
            cancellationToken);

        return Result.Success();
    }
}
