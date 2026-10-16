using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Events;

/// <summary>
/// Shared by all three appointment event handlers below: looks up the
/// doctor's linked login (if any) and persists+pushes a Notification. Kept
/// as a base class rather than three copies of the same lookup/persist/push
/// sequence.
/// </summary>
public abstract class AppointmentNotificationHandlerBase
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly ICurrentUserService CurrentUserService;
    protected readonly INotificationDispatcher Dispatcher;

    protected AppointmentNotificationHandlerBase(
        IUnitOfWork unitOfWork, ICurrentUserService currentUserService, INotificationDispatcher dispatcher)
    {
        UnitOfWork = unitOfWork;
        CurrentUserService = currentUserService;
        Dispatcher = dispatcher;
    }

    protected async Task NotifyDoctorAsync(
        Guid doctorId, string title, string message, NotificationType type, string relatedEntityId, CancellationToken cancellationToken)
    {
        var doctor = await UnitOfWork.Repository<Doctor>().Query()
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

        if (doctor?.ApplicationUserId is not { } userId) return; // doctor has no login account linked yet

        await UnitOfWork.Repository<Notification>().AddAsync(new Notification
        {
            TenantId = CurrentUserService.TenantId,
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedEntityId = relatedEntityId
        }, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        await Dispatcher.SendToUserAsync(userId, title, message, type.ToString(), cancellationToken);
    }
}

public class AppointmentCreatedEventHandler : AppointmentNotificationHandlerBase, INotificationHandler<AppointmentCreatedEvent>
{
    public AppointmentCreatedEventHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, INotificationDispatcher dispatcher)
        : base(unitOfWork, currentUserService, dispatcher) { }

    public Task Handle(AppointmentCreatedEvent notification, CancellationToken cancellationToken) =>
        NotifyDoctorAsync(
            notification.DoctorId,
            "New Appointment",
            $"A new appointment has been booked for {notification.ScheduledStart:g}.",
            NotificationType.NewAppointment,
            notification.AppointmentId.ToString(),
            cancellationToken);
}

public class AppointmentCancelledEventHandler : AppointmentNotificationHandlerBase, INotificationHandler<AppointmentCancelledEvent>
{
    public AppointmentCancelledEventHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, INotificationDispatcher dispatcher)
        : base(unitOfWork, currentUserService, dispatcher) { }

    public Task Handle(AppointmentCancelledEvent notification, CancellationToken cancellationToken) =>
        NotifyDoctorAsync(
            notification.DoctorId,
            "Appointment Cancelled",
            $"An appointment was cancelled: {notification.Reason}",
            NotificationType.AppointmentCancelled,
            notification.AppointmentId.ToString(),
            cancellationToken);
}

public class AppointmentRescheduledEventHandler : AppointmentNotificationHandlerBase, INotificationHandler<AppointmentRescheduledEvent>
{
    public AppointmentRescheduledEventHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, INotificationDispatcher dispatcher)
        : base(unitOfWork, currentUserService, dispatcher) { }

    public Task Handle(AppointmentRescheduledEvent notification, CancellationToken cancellationToken) =>
        NotifyDoctorAsync(
            notification.DoctorId,
            "Appointment Rescheduled",
            $"An appointment was moved to {notification.NewStart:g}.",
            NotificationType.AppointmentRescheduled,
            notification.AppointmentId.ToString(),
            cancellationToken);
}
