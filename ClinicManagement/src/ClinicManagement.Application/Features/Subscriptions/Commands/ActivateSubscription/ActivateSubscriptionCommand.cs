using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Subscriptions.Events;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Subscriptions.Commands.ActivateSubscription;

/// <summary>
/// The subscription-side counterpart of RecordGatewayPaymentCommand. Called only from
/// StripeWebhookController — like that handler, it deliberately does not use
/// ICurrentUserService.TenantId (a webhook has no JWT); TenantId comes from the checkout
/// session's own metadata (set in CreateSubscriptionCheckoutCommand) and is verified against
/// the pending Subscription it claims to be paying for, so a forged/mismatched TenantId can't
/// activate a plan for the wrong tenant.
/// </summary>
public record ActivateSubscriptionCommand(Guid TenantId, Guid SubscriptionId, decimal AmountPaid, string ProviderReference)
    : IRequest<Result>;

public class ActivateSubscriptionCommandHandler : IRequestHandler<ActivateSubscriptionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public ActivateSubscriptionCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<Result> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscriptionRepo = _unitOfWork.Repository<Subscription>();
        var subscription = await subscriptionRepo.Query()
            .IgnoreQueryFilters()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(
                s => s.Id == request.SubscriptionId && s.TenantId == request.TenantId, cancellationToken);

        if (subscription is null)
        {
            return Result.Failure($"Subscription '{request.SubscriptionId}' was not found for tenant '{request.TenantId}'.");
        }

        // Idempotency guard: Stripe can (and does) redeliver the same webhook event more than
        // once. A payment already recorded with this exact provider reference means this is a
        // redelivery, not a second payment — succeed without doing anything twice.
        var paymentRepo = _unitOfWork.Repository<SubscriptionPayment>();
        var alreadyRecorded = await paymentRepo.Query()
            .IgnoreQueryFilters()
            .AnyAsync(p => p.TenantId == request.TenantId && p.TransactionReference == request.ProviderReference, cancellationToken);
        if (alreadyRecorded)
        {
            return Result.Success();
        }

        // Only a still-pending checkout should be activated. If the tenant cancelled, or this
        // row was already superseded by a later checkout attempt, there's nothing to do — the
        // money still arrived, but silently no-op-ing here is safer than resurrecting a
        // subscription the tenant no longer expects to be on.
        if (subscription.Status != SubscriptionStatus.PendingPayment)
        {
            return Result.Success();
        }

        // Supersede whatever the tenant was on before, same as UpgradeSubscriptionCommand —
        // each plan period stays its own row for billing history.
        var previousActive = await subscriptionRepo.Query()
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == request.TenantId
                        && s.Id != subscription.Id
                        && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .ToListAsync(cancellationToken);
        foreach (var previous in previousActive)
        {
            previous.Status = SubscriptionStatus.Cancelled;
            previous.AutoRenew = false;
            subscriptionRepo.Update(previous);
        }

        subscription.StartDate = DateTime.UtcNow;
        subscription.EndDate = DateTime.UtcNow.AddDays(subscription.Plan.DurationInDays);
        subscription.Status = SubscriptionStatus.Active;
        subscriptionRepo.Update(subscription);

        await paymentRepo.AddAsync(new SubscriptionPayment
        {
            TenantId = request.TenantId,
            SubscriptionId = subscription.Id,
            Amount = request.AmountPaid,
            Method = PaymentMethod.Card,
            PaidAt = DateTime.UtcNow,
            TransactionReference = request.ProviderReference
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new SubscriptionActivatedEvent(request.TenantId, subscription.Plan.Name, subscription.EndDate),
            cancellationToken);

        return Result.Success();
    }
}
