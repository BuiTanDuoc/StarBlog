using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Subscriptions.Commands.CancelSubscription;

/// <summary>
/// Cancels auto-renewal. The tenant keeps access until the already-paid-for
/// EndDate — this is not an immediate cutoff (that's what letting
/// ExpiredSubscriptionsJob naturally expire it achieves). No request body:
/// always acts on the caller's own tenant.
/// </summary>
public record CancelSubscriptionCommand : IRequest<Result>;

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CancelSubscriptionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Subscription>();
        var subscription = await repository.Query()
            .Where(s => s.TenantId == _currentUserService.TenantId
                        && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is null)
        {
            return Result.Failure("No active subscription to cancel.");
        }

        subscription.AutoRenew = false;
        repository.Update(subscription);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
