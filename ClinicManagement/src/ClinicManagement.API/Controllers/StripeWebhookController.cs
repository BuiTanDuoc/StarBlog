using ClinicManagement.Application.Features.Invoices.Commands.RecordGatewayPayment;
using ClinicManagement.Application.Features.Subscriptions.Commands.ActivateSubscription;
using ClinicManagement.Infrastructure.Options;
using ClinicManagement.Infrastructure.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ClinicManagement.API.Controllers;

/// <summary>
/// Anonymous by necessity — Stripe calls this directly, it has no user JWT.
/// The signature check (StripeWebhookVerifier, using Stripe:WebhookSigningSecret)
/// is what stands in for authentication here: anyone who doesn't know that
/// secret can't produce a header this endpoint accepts. Never trust the JSON
/// body's contents (including its metadata) without that check passing first.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/webhooks/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly StripeSettings _settings;
    private readonly ISender _sender;

    public StripeWebhookController(IOptions<StripeSettings> settings, ISender sender)
    {
        _settings = settings.Value;
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Handle()
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var payload = await reader.ReadToEndAsync();

        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
        if (!StripeWebhookVerifier.Verify(payload, signatureHeader, _settings.WebhookSigningSecret))
        {
            return Unauthorized();
        }

        using var doc = JsonDocument.Parse(payload);
        var eventType = doc.RootElement.GetProperty("type").GetString();

        // Only the event that means "money has actually moved" — other checkout.session.*
        // events (expired, async_payment_failed, ...) aren't a payment and are ignored.
        if (eventType == "checkout.session.completed")
        {
            var session = doc.RootElement.GetProperty("data").GetProperty("object");
            var metadata = session.GetProperty("metadata");

            var tenantId = Guid.Parse(metadata.GetProperty("tenantId").GetString()!);
            var amountTotalCents = session.GetProperty("amount_total").GetInt64();
            var sessionId = session.GetProperty("id").GetString()!;
            var amount = amountTotalCents / 100m;

            // Two different flows share this one webhook endpoint, told apart by which id the
            // checkout session's metadata carries — "invoiceId" for a patient paying their bill
            // (CreateInvoiceCheckoutCommand), "subscriptionId" for a clinic paying for its plan
            // (CreateSubscriptionCheckoutCommand). Never both on the same session.
            if (metadata.TryGetProperty("subscriptionId", out var subscriptionIdProp))
            {
                var subscriptionId = Guid.Parse(subscriptionIdProp.GetString()!);
                await _sender.Send(new ActivateSubscriptionCommand(tenantId, subscriptionId, amount, sessionId));
            }
            else if (metadata.TryGetProperty("invoiceId", out var invoiceIdProp))
            {
                var invoiceId = Guid.Parse(invoiceIdProp.GetString()!);
                await _sender.Send(new RecordGatewayPaymentCommand(tenantId, invoiceId, amount, sessionId));
            }
        }

        // Stripe only cares about the 2xx — always acknowledge once the signature checks out,
        // even for event types we don't act on, so it doesn't keep retrying forever.
        return Ok();
    }
}
