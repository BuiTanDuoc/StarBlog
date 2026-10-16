using System.Net.Http.Headers;
using System.Text.Json;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Payments;

/// <summary>
/// Calls Stripe's Checkout Sessions REST API
/// (https://docs.stripe.com/api/checkout/sessions/create) directly via
/// HttpClient rather than the Stripe .NET SDK, so this compiles without a
/// package restore in environments that can't reach nuget.org — same
/// reasoning as TwilioSmsSender. To use a different provider (PayTabs,
/// Fawry, ...), implement IPaymentGatewayService the same way; nothing
/// above this class (CreateInvoiceCheckoutCommand, the controllers) needs
/// to change.
/// </summary>
public class StripePaymentGatewayService : IPaymentGatewayService
{
    private const string ApiBase = "https://api.stripe.com/v1";

    private readonly StripeSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;

    public StripePaymentGatewayService(IOptions<StripeSettings> settings, IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        CheckoutSessionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            // Fails loudly on purpose — see the class comment on StripeSettings.
            throw new InvalidOperationException(
                "Stripe is not configured (Stripe:SecretKey is empty). Set it in appsettings before enabling online payments.");
        }

        var client = _httpClientFactory.CreateClient(nameof(StripePaymentGatewayService));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.SecretKey);

        // Stripe's checkout/sessions endpoint takes application/x-www-form-urlencoded with
        // bracket-notation keys for nested objects/arrays — there's no JSON body option.
        var unitAmount = (long)Math.Round(request.AmountDue * 100, MidpointRounding.AwayFromZero);
        var form = new Dictionary<string, string>
        {
            ["mode"] = "payment",
            ["success_url"] = request.SuccessUrl,
            ["cancel_url"] = request.CancelUrl,
            ["line_items[0][price_data][currency]"] = _settings.DefaultCurrency,
            ["line_items[0][price_data][product_data][name]"] = $"Invoice {request.InvoiceNumber}",
            ["line_items[0][price_data][unit_amount]"] = unitAmount.ToString(),
            ["line_items[0][quantity]"] = "1",
            ["metadata[invoiceId]"] = request.InvoiceId.ToString(),
            ["metadata[tenantId]"] = request.TenantId.ToString(),
        };
        if (!string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            form["customer_email"] = request.CustomerEmail;
        }

        var response = await client.PostAsync(
            $"{ApiBase}/checkout/sessions", new FormUrlEncodedContent(form), cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Stripe checkout session creation failed ({response.StatusCode}): {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var sessionId = doc.RootElement.GetProperty("id").GetString()!;
        var checkoutUrl = doc.RootElement.GetProperty("url").GetString()!;

        return new CheckoutSessionResult(checkoutUrl, sessionId);
    }

    /// <summary>
    /// Deliberately mode=payment (a single one-off charge), not mode=subscription —
    /// this codebase bills subscriptions in fixed-length periods that ActivateSubscriptionCommand
    /// creates one at a time, not via Stripe's own recurring-billing engine. Keeps subscription
    /// renewals working the same way (checkout → webhook → activate) regardless of gateway.
    /// </summary>
    public async Task<CheckoutSessionResult> CreateSubscriptionCheckoutSessionAsync(
        SubscriptionCheckoutSessionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            throw new InvalidOperationException(
                "Stripe is not configured (Stripe:SecretKey is empty). Set it in appsettings before enabling online payments.");
        }

        var client = _httpClientFactory.CreateClient(nameof(StripePaymentGatewayService));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.SecretKey);

        var unitAmount = (long)Math.Round(request.Amount * 100, MidpointRounding.AwayFromZero);
        var form = new Dictionary<string, string>
        {
            ["mode"] = "payment",
            ["success_url"] = request.SuccessUrl,
            ["cancel_url"] = request.CancelUrl,
            ["line_items[0][price_data][currency]"] = _settings.DefaultCurrency,
            ["line_items[0][price_data][product_data][name]"] = $"Subscription — {request.PlanName}",
            ["line_items[0][price_data][unit_amount]"] = unitAmount.ToString(),
            ["line_items[0][quantity]"] = "1",
            // "subscriptionId" is what StripeWebhookController uses to tell a subscription
            // checkout apart from an invoice checkout (which stamps "invoiceId" instead).
            ["metadata[subscriptionId]"] = request.SubscriptionId.ToString(),
            ["metadata[tenantId]"] = request.TenantId.ToString(),
        };
        if (!string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            form["customer_email"] = request.CustomerEmail;
        }

        var response = await client.PostAsync(
            $"{ApiBase}/checkout/sessions", new FormUrlEncodedContent(form), cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Stripe checkout session creation failed ({response.StatusCode}): {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var sessionId = doc.RootElement.GetProperty("id").GetString()!;
        var checkoutUrl = doc.RootElement.GetProperty("url").GetString()!;

        return new CheckoutSessionResult(checkoutUrl, sessionId);
    }
}
