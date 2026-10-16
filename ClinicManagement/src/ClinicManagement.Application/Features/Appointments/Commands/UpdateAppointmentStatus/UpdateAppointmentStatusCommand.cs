using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.UpdateAppointmentStatus;

public record UpdateAppointmentStatusCommand(Guid Id, AppointmentStatus NewStatus, string? Reason) : IRequest<Result>;

public class UpdateAppointmentStatusCommandValidator : AbstractValidator<UpdateAppointmentStatusCommand>
{
    public UpdateAppointmentStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}

public class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand, Result>
{
    /// <summary>Legal forward transitions. Cancellation/no-show have their own dedicated commands
    /// (CancelAppointment) or are reachable from any non-terminal state, handled below.</summary>
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> AllowedTransitions = new()
    {
        [AppointmentStatus.Pending] = new[] { AppointmentStatus.Confirmed, AppointmentStatus.Cancelled },
        [AppointmentStatus.Confirmed] = new[] { AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow },
        [AppointmentStatus.CheckedIn] = new[] { AppointmentStatus.InProgress, AppointmentStatus.Cancelled },
        [AppointmentStatus.InProgress] = new[] { AppointmentStatus.Completed },
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateAppointmentStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Appointment>();
        var appointment = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (appointment is null)
        {
            return Result.Failure($"Appointment with id '{request.Id}' was not found.");
        }

        if (!AllowedTransitions.TryGetValue(appointment.Status, out var allowed) || !allowed.Contains(request.NewStatus))
        {
            return Result.Failure($"Cannot transition an appointment from '{appointment.Status}' to '{request.NewStatus}'.");
        }

        var previousStatus = appointment.Status;
        appointment.Status = request.NewStatus;

        appointment.StatusHistory.Add(new AppointmentStatusHistory
        {
            TenantId = _currentUserService.TenantId,
            AppointmentId = appointment.Id,
            FromStatus = previousStatus,
            ToStatus = request.NewStatus,
            ChangedBy = _currentUserService.UserName,
            Reason = request.Reason
        });

        repository.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
