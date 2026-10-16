import { useSearchParams, Link } from "react-router-dom";
import { useTranslation } from "react-i18next";

/**
 * Stripe redirects the browser here after checkout — but the redirect
 * itself is not proof of payment (the customer could bail after Stripe
 * shows its own success screen, or the browser could lose connectivity).
 * The real source of truth is the "checkout.session.completed" webhook
 * (see StripeWebhookController), which updates the invoice or subscription
 * independently and usually arrives within seconds. This page intentionally
 * doesn't claim "payment successful" — it tells the person what actually
 * happened (they finished/cancelled Stripe's flow) and points them back to
 * check the real status a moment later. Shared by both the invoice-payment
 * flow and the subscription-checkout flow, so the copy stays generic rather
 * than naming either one specifically.
 */
export function CheckoutResultPage({ outcome }: { outcome: "success" | "cancelled" }) {
  const { t } = useTranslation();
  const [params] = useSearchParams();
  const backTo = params.get("back") ?? "/";

  return (
    <div className="flex min-h-screen items-center justify-center bg-canvas px-6">
      <div className="max-w-sm text-center">
        <p className="font-display text-2xl italic text-ink">
          {outcome === "success" ? t("billing.thanks") : t("billing.checkoutCancelled")}
        </p>
        <p className="mt-3 text-sm text-ink-muted">
          {outcome === "success" ? t("billing.paymentBeingConfirmed") : t("billing.noPaymentMade")}
        </p>
        <Link to={backTo} className="mt-6 inline-block text-sm font-medium text-harbor-500 hover:underline">
          {t("common.back")}
        </Link>
      </div>
    </div>
  );
}
