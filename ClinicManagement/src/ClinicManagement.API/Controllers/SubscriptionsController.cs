using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Subscriptions.Commands.CancelSubscription;
using ClinicManagement.Application.Features.Subscriptions.Commands.CreateSubscriptionCheckout;
using ClinicManagement.Application.Features.Subscriptions.Commands.UpgradeSubscription;
using ClinicManagement.Application.Features.Subscriptions.Queries.GetCurrentSubscription;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class SubscriptionsController : ApiControllerBase
{
    /// <summary>Current plan + live usage stats (e.g. "3 of 10 doctors used") for the ClinicOwner dashboard.</summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent() =>
        HandleResult(await Mediator.Send(new GetCurrentSubscriptionQuery()));

    /// <summary>Free plans only — switches immediately, no payment involved. Paid plans must use /checkout.</summary>
    [HasPermission(Permissions.Tenants.ManageSubscriptions)]
    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade(UpgradeSubscriptionCommand command) =>
        HandleResult(await Mediator.Send(command));

    /// <summary>
    /// Starts a hosted Stripe checkout for a paid plan. The subscription only becomes
    /// Active once StripeWebhookController's "checkout.session.completed" handling calls
    /// ActivateSubscriptionCommand — this endpoint just hands back where to send the browser.
    /// </summary>
    [HasPermission(Permissions.Tenants.ManageSubscriptions)]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CreateSubscriptionCheckoutBody body) =>
        HandleResult(await Mediator.Send(new CreateSubscriptionCheckoutCommand(body.NewPlanId, body.SuccessUrl, body.CancelUrl)));

    [HasPermission(Permissions.Tenants.ManageSubscriptions)]
    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel() =>
        HandleResult(await Mediator.Send(new CancelSubscriptionCommand()));

    public record CreateSubscriptionCheckoutBody(Guid NewPlanId, string SuccessUrl, string CancelUrl);
}
