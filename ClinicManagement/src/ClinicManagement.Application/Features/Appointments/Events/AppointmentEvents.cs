using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Events;

public record AppointmentCreatedEvent(Guid AppointmentId, Guid DoctorId, Guid PatientId, DateTime ScheduledStart) : INotification;

public record AppointmentCancelledEvent(Guid AppointmentId, Guid DoctorId, Guid PatientId, string Reason) : INotification;

public record AppointmentRescheduledEvent(Guid AppointmentId, Guid DoctorId, Guid PatientId, DateTime NewStart) : INotification;
