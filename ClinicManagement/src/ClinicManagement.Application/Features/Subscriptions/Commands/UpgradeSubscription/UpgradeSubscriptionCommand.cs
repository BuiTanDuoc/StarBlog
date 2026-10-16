using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Subscriptions.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Subscriptions.Commands.UpgradeSubscription;

/// <summary>
/// Switches the caller's tenant to a different plan effective immediately.
/// Only for free plans (Price == 0) — there's no money changing hands, so no gateway
/// round-trip is needed and it's safe to activate synchronously. Paid plans must go
/// through CreateSubscriptionCheckoutCommand instead, which only activates the
/// subscription once the gateway webhook (see ActivateSubscriptionCommand) confirms
/// payment actually happened; this command no longer trusts a client-supplied
/// PaymentReference for paid plans, since that would let anyone "pay" without paying.
/// Downgrades are allowed too: existing records over the new plan's limits are left
/// untouched (see UpdatePlanCommand's note on the same trade-off), only new creation
/// is blocked going forward.
/// </summary>
public record UpgradeSubscriptionCommand(Guid NewPlanId) : IRequest<Result<SubscriptionDto>>;

public class UpgradeSubscriptionCommandValidator : AbstractValidator<UpgradeSubscriptionCommand>
{
    public UpgradeSubscriptionCommandValidator()
    {
        RuleFor(x => x.NewPlanId).NotEmpty();
    }
}

public class UpgradeSubscriptionCommandHandler : IRequestHandler<UpgradeSubscriptionCommand, Result<SubscriptionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public UpgradeSubscriptionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<SubscriptionDto>> Handle(UpgradeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var newPlan = await _unitOfWork.Repository<Plan>().Query()
            .FirstOrDefaultAsync(p => p.Id == request.NewPlanId && p.IsActive, cancellationToken);

        if (newPlan is null)
        {
            return Result<SubscriptionDto>.Failure("Selected plan was not found or is no longer available.");
        }

        if (newPlan.Price > 0)
        {
            return Result<SubscriptionDto>.Failure(
                "This plan requires payment — start a checkout session instead of switching directly.");
        }

        var subscriptionRepo = _unitOfWork.Repository<Subscription>();
        var currentSubscription = await subscriptionRepo.Query()
            .Where(s => s.TenantId == _currentUserService.TenantId
                        && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        // Superseding the old subscription (rather than mutating its PlanId in place) keeps
        // an accurate billing history — each plan period is its own row.
        if (currentSubscription is not null)
        {
            currentSubscription.Status = SubscriptionStatus.Cancelled;
            currentSubscription.AutoRenew = false;
            subscriptionRepo.Update(currentSubscription);
        }

        var newSubscription = new Subscription
        {
            TenantId = _currentUserService.TenantId,
            PlanId = newPlan.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(newPlan.DurationInDays),
            Status = SubscriptionStatus.Active,
            AutoRenew = true
        };
        await subscriptionRepo.AddAsync(newSubscription, cancellationToken);

        // No SubscriptionPayment row here — this plan is free (guarded above), so there's
        // nothing paid to record. Paid plans get their SubscriptionPayment from
        // ActivateSubscriptionCommand once the gateway confirms the charge.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        newSubscription.Plan = newPlan; // avoid a round-trip just to populate the DTO's PlanName/PlanTier
        return Result<SubscriptionDto>.Success(_mapper.Map<SubscriptionDto>(newSubscription));
    }
}
