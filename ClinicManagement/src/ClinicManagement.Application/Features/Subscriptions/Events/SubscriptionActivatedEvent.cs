using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Subscriptions.Events;

public record SubscriptionActivatedEvent(Guid TenantId, string PlanName, DateTime EndDate) : INotification;

/// <summary>
/// Same reasoning as PaymentReceivedEventHandler — a subscription isn't owned by
/// one staff member, so this pushes straight to the tenant's live dashboards
/// rather than a persisted per-user Notification row.
/// </summary>
public class SubscriptionActivatedEventHandler : INotificationHandler<SubscriptionActivatedEvent>
{
    private readonly INotificationDispatcher _dispatcher;

    public SubscriptionActivatedEventHandler(INotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public Task Handle(SubscriptionActivatedEvent notification, CancellationToken cancellationToken) =>
        _dispatcher.SendToTenantAsync(
            notification.TenantId,
            "Subscription Activated",
            $"Payment confirmed — your clinic is now on the {notification.PlanName} plan until {notification.EndDate:yyyy-MM-dd}.",
            NotificationType.General.ToString(),
            cancellationToken);
}
