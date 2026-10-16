using System.Security.Cryptography;
using System.Text;

namespace ClinicManagement.Infrastructure.Payments;

/// <summary>
/// Verifies the "Stripe-Signature" header Stripe attaches to every webhook
/// POST (https://docs.stripe.com/webhooks#verify-manually), implemented by
/// hand rather than via the Stripe SDK — see StripePaymentGatewayService for
/// why. Format: "t=&lt;unix-timestamp&gt;,v1=&lt;hex-hmac&gt;[,v1=...]" — Stripe can send
/// multiple v1 values during secret rotation, so any match is accepted.
/// </summary>
public static class StripeWebhookVerifier
{
    private static readonly TimeSpan ToleranceWindow = TimeSpan.FromMinutes(5);

    public static bool Verify(string payload, string? signatureHeader, string webhookSecret)
    {
        if (string.IsNullOrEmpty(signatureHeader) || string.IsNullOrEmpty(webhookSecret))
        {
            return false;
        }

        string? timestampPart = null;
        var candidateSignatures = new List<string>();

        foreach (var element in signatureHeader.Split(','))
        {
            var kv = element.Split('=', 2);
            if (kv.Length != 2) continue;

            if (kv[0] == "t") timestampPart = kv[1];
            else if (kv[0] == "v1") candidateSignatures.Add(kv[1]);
        }

        if (timestampPart is null || candidateSignatures.Count == 0)
        {
            return false;
        }

        if (!long.TryParse(timestampPart, out var timestampUnix))
        {
            return false;
        }

        var timestamp = DateTimeOffset.FromUnixTimeSeconds(timestampUnix);
        if (Math.Abs((DateTimeOffset.UtcNow - timestamp).TotalSeconds) > ToleranceWindow.TotalSeconds)
        {
            // Old enough to plausibly be a replayed request — reject even if the signature
            // itself is otherwise valid for that (stale) payload+timestamp combination.
            return false;
        }

        var signedPayload = $"{timestampPart}.{payload}";
        var expectedBytes = HMACSHA256.HashData(Encoding.UTF8.GetBytes(webhookSecret), Encoding.UTF8.GetBytes(signedPayload));
        var expectedHex = Convert.ToHexStringLower(expectedBytes);

        return candidateSignatures.Any(candidate =>
            CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(candidate), Encoding.UTF8.GetBytes(expectedHex)));
    }
}
