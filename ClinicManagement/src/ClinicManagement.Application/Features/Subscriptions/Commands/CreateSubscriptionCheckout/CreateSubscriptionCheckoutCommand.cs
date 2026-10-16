using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Subscriptions.Commands.CreateSubscriptionCheckout;

/// <summary>
/// Starts real payment for a paid plan. Unlike UpgradeSubscriptionCommand (which
/// books a subscription as already-paid), this never activates anything itself —
/// it creates a placeholder Subscription row in PendingPayment and hands back a
/// Stripe checkout link. Activation only happens when StripeWebhookController
/// receives "checkout.session.completed" and forwards it to ActivateSubscriptionCommand.
/// Free plans (Price == 0) have nothing to pay for and should go through
/// UpgradeSubscriptionCommand instead — this command rejects them.
/// </summary>
public record CreateSubscriptionCheckoutCommand(Guid NewPlanId, string SuccessUrl, string CancelUrl)
    : IRequest<Result<CheckoutSessionResult>>;

public class CreateSubscriptionCheckoutCommandValidator : AbstractValidator<CreateSubscriptionCheckoutCommand>
{
    public CreateSubscriptionCheckoutCommandValidator()
    {
        RuleFor(x => x.NewPlanId).NotEmpty();
        RuleFor(x => x.SuccessUrl).NotEmpty();
        RuleFor(x => x.CancelUrl).NotEmpty();
    }
}

public class CreateSubscriptionCheckoutCommandHandler
    : IRequestHandler<CreateSubscriptionCheckoutCommand, Result<CheckoutSessionResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentGatewayService _paymentGatewayService;

    public CreateSubscriptionCheckoutCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IPaymentGatewayService paymentGatewayService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _paymentGatewayService = paymentGatewayService;
    }

    public async Task<Result<CheckoutSessionResult>> Handle(
        CreateSubscriptionCheckoutCommand request, CancellationToken cancellationToken)
    {
        var plan = await _unitOfWork.Repository<Plan>().Query()
            .FirstOrDefaultAsync(p => p.Id == request.NewPlanId && p.IsActive, cancellationToken);

        if (plan is null)
        {
            return Result<CheckoutSessionResult>.Failure("Selected plan was not found or is no longer available.");
        }

        if (plan.Price <= 0)
        {
            return Result<CheckoutSessionResult>.Failure(
                "This plan is free — use the upgrade endpoint directly, no payment is needed.");
        }

        var subscriptionRepo = _unitOfWork.Repository<Subscription>();

        // Abandoned checkout attempts (started, never paid, tried again) shouldn't pile up as
        // dangling PendingPayment rows — clear them before starting a fresh one.
        var stalePending = await subscriptionRepo.Query()
            .Where(s => s.TenantId == _currentUserService.TenantId && s.Status == SubscriptionStatus.PendingPayment)
            .ToListAsync(cancellationToken);
        foreach (var stale in stalePending)
        {
            stale.Status = SubscriptionStatus.Cancelled;
            subscriptionRepo.Update(stale);
        }

        // Placeholder dates — ActivateSubscriptionCommand sets the real StartDate/EndDate once
        // payment is confirmed, so a tenant can't extend their access just by opening checkout.
        var pendingSubscription = new Subscription
        {
            TenantId = _currentUserService.TenantId,
            PlanId = plan.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow,
            Status = SubscriptionStatus.PendingPayment,
            AutoRenew = true
        };
        await subscriptionRepo.AddAsync(pendingSubscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var tenant = await _unitOfWork.Repository<Tenant>().Query()
            .FirstOrDefaultAsync(t => t.Id == _currentUserService.TenantId, cancellationToken);

        var session = await _paymentGatewayService.CreateSubscriptionCheckoutSessionAsync(
            new SubscriptionCheckoutSessionRequest(
                TenantId: _currentUserService.TenantId,
                SubscriptionId: pendingSubscription.Id,
                PlanName: plan.Name,
                Amount: plan.Price,
                CustomerEmail: tenant?.ContactEmail,
                SuccessUrl: request.SuccessUrl,
                CancelUrl: request.CancelUrl),
            cancellationToken);

        return Result<CheckoutSessionResult>.Success(session);
    }
}
