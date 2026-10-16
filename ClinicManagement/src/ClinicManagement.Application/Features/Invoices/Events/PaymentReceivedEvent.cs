using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Invoices.Events;

public record PaymentReceivedEvent(Guid TenantId, Guid InvoiceId, Guid PatientId, decimal Amount) : INotification;

/// <summary>
/// Unlike the appointment events, this pushes straight to the tenant group
/// rather than persisting a per-user Notification row — a payment isn't
/// "owned" by one staff member the way an appointment is owned by its
/// doctor, so there's no single obvious UserId to attach a Notification
/// entity to. Any connected dashboard (receptionist, accountant, owner)
/// picks this up live; it just isn't sitting in anyone's notification inbox
/// afterwards. Revisit if a persisted billing-activity feed is needed.
/// </summary>
public class PaymentReceivedEventHandler : INotificationHandler<PaymentReceivedEvent>
{
    private readonly INotificationDispatcher _dispatcher;

    public PaymentReceivedEventHandler(INotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public Task Handle(PaymentReceivedEvent notification, CancellationToken cancellationToken) =>
        _dispatcher.SendToTenantAsync(
            notification.TenantId,
            "Payment Received",
            $"A payment of {notification.Amount:C} was recorded.",
            NotificationType.PaymentReceived.ToString(),
            cancellationToken);
}
