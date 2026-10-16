namespace ClinicManagement.Infrastructure.Options;

/// <summary>
/// Bound from the "Stripe" section of appsettings.json. If SecretKey is
/// empty, StripePaymentGatewayService fails fast with a clear error instead
/// of silently no-op'ing (unlike the reminder senders) — a checkout button
/// that appears to work but never actually charges anyone is worse than one
/// that visibly errors, since money is involved.
/// </summary>
public class StripeSettings
{
    public const string SectionName = "Stripe";

    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Used to verify the "Stripe-Signature" header on incoming webhooks — see StripeWebhookController.</summary>
    public string WebhookSigningSecret { get; set; } = string.Empty;

    public string DefaultCurrency { get; set; } = "usd";
}
