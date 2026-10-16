namespace ClinicManagement.Application.Common.Interfaces;

public record CheckoutSessionRequest(
    Guid TenantId,
    Guid InvoiceId,
    string InvoiceNumber,
    decimal AmountDue,
    string? CustomerEmail,
    string SuccessUrl,
    string CancelUrl);

public record CheckoutSessionResult(string CheckoutUrl, string ProviderSessionId);

/// <summary>
/// Same idea as CheckoutSessionRequest but for a clinic paying for its own
/// subscription plan, not a patient paying an invoice. Kept as a distinct
/// record (rather than reusing CheckoutSessionRequest with a nullable
/// InvoiceId) so the metadata each flow puts on the Stripe session — and
/// therefore what the webhook has to branch on — stays unambiguous.
/// </summary>
public record SubscriptionCheckoutSessionRequest(
    Guid TenantId,
    Guid SubscriptionId,
    string PlanName,
    decimal Amount,
    string? CustomerEmail,
    string SuccessUrl,
    string CancelUrl);

/// <summary>
/// One checkout-session-based abstraction that Stripe, PayTabs, or Fawry can
/// all implement the same way (they're all "redirect the customer to a
/// hosted payment page, then tell us via webhook") — see
/// StripePaymentGatewayService in Infrastructure for the reference
/// implementation. Swapping providers means implementing this interface
/// again, not touching CreateInvoiceCheckoutCommand or the webhook plumbing.
/// </summary>
public interface IPaymentGatewayService
{
    Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        CheckoutSessionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Same Checkout Sessions API, different line item — a subscription's plan price instead of an invoice's balance.</summary>
    Task<CheckoutSessionResult> CreateSubscriptionCheckoutSessionAsync(
        SubscriptionCheckoutSessionRequest request, CancellationToken cancellationToken = default);
}
