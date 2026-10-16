import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import { api, ApiError } from "../../lib/api";
import {
  SUBSCRIPTION_STATUS_STYLES,
  translatePlanTier,
  translateSubscriptionStatus,
  formatCurrency,
  formatDate,
} from "../../lib/format";
import { primaryButtonClass, secondaryButtonClass } from "../../lib/ui";

interface UsageStat {
  used: number;
  max: number;
  isUnlimited: boolean;
  percentUsed: number;
}

interface SubscriptionDto {
  id: string;
  planId: string;
  planName: string;
  planTier: string;
  startDate: string;
  endDate: string;
  status: string;
  autoRenew: boolean;
}

interface SubscriptionWithUsage {
  subscription: SubscriptionDto;
  doctors: UsageStat;
  employees: UsageStat;
  patients: UsageStat;
  appointmentsThisMonth: UsageStat;
}

interface PlanDto {
  id: string;
  name: string;
  tier: string;
  price: number;
  durationInDays: number;
  maxDoctors: number;
  maxEmployees: number;
  maxPatients: number;
  maxAppointmentsPerMonth: number;
}

function UsageBar({ label, stat }: { label: string; stat: UsageStat }) {
  const { t } = useTranslation();
  return (
    <div>
      <div className="flex items-center justify-between text-xs text-ink-muted">
        <span>{label}</span>
        <span className="tabular-nums">
          {stat.used} / {stat.isUnlimited ? t("subscription.unlimitedSymbol") : stat.max}
        </span>
      </div>
      <div className="mt-1 h-1.5 overflow-hidden rounded-full bg-line">
        <div
          className={`h-full rounded-full ${stat.percentUsed >= 90 ? "bg-danger" : "bg-harbor-500"}`}
          style={{ width: `${stat.isUnlimited ? 8 : Math.min(100, stat.percentUsed)}%` }}
        />
      </div>
    </div>
  );
}

export function SubscriptionPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];
  const canManage = roles.some((r) => ["ClinicOwner", "SuperAdmin"].includes(r));

  const { data, isLoading, error, refetch } = useApiData<SubscriptionWithUsage>("/api/subscriptions/current");
  const { data: plans } = useApiData<PlanDto[]>("/api/plans?activeOnly=true");

  const [busyPlanId, setBusyPlanId] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [isCancelling, setIsCancelling] = useState(false);

  async function handleUpgrade(plan: PlanDto) {
    setBusyPlanId(plan.id);
    setActionError(null);
    try {
      if (plan.price > 0) {
        // Paid plan: hand off to Stripe. The plan only actually switches once the gateway
        // webhook confirms payment (see ActivateSubscriptionCommand) — this just starts checkout.
        const origin = window.location.origin;
        const response = await api.post<{ checkoutUrl: string }>("/api/subscriptions/checkout", {
          newPlanId: plan.id,
          successUrl: `${origin}/billing/checkout-success?back=/subscription`,
          cancelUrl: `${origin}/billing/checkout-cancelled?back=/subscription`,
        });
        window.location.href = response.checkoutUrl;
        return;
      }

      // Free plan: no payment involved, switches immediately.
      await api.post("/api/subscriptions/upgrade", { newPlanId: plan.id });
      refetch();
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : t("subscription.failedToSwitch"));
    } finally {
      setBusyPlanId(null);
    }
  }

  async function handleCancel() {
    setIsCancelling(true);
    setActionError(null);
    try {
      await api.post("/api/subscriptions/cancel");
      refetch();
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : t("subscription.failedToCancelRenewal"));
    } finally {
      setIsCancelling(false);
    }
  }

  return (
    <AppShell title={t("nav.subscription")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-6">
        {isLoading && <p className="py-8 text-center font-mono text-sm text-ink-muted">{t("subscription.loadingSubscription")}</p>}
        {error && <p className="text-sm text-danger">{error}</p>}
        {actionError && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{actionError}</p>}

        {data && (
          <div className="rounded-lg border border-line bg-surface p-6 shadow-card">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <p className="text-xs uppercase tracking-[0.06em] text-ink-faint">{t("subscription.currentPlan")}</p>
                <h2 className="mt-1 font-display text-xl text-ink">
                  {data.subscription.planName}{" "}
                  <span className="font-sans text-sm font-normal text-ink-faint">
                    ({translatePlanTier(data.subscription.planTier, t)})
                  </span>
                </h2>
                <p className="mt-1 text-sm text-ink-muted">
                  {formatDate(data.subscription.startDate)} – {formatDate(data.subscription.endDate)}
                  {data.subscription.autoRenew ? ` · ${t("subscription.autoRenews")}` : ` · ${t("subscription.willNotRenew")}`}
                </p>
              </div>
              <span
                className={`rounded-full px-2.5 py-1 text-xs font-medium ${SUBSCRIPTION_STATUS_STYLES[data.subscription.status] ?? "bg-line text-ink-muted"}`}
              >
                {translateSubscriptionStatus(data.subscription.status, t)}
              </span>
            </div>

            <div className="mt-5 grid grid-cols-1 gap-4 sm:grid-cols-2">
              <UsageBar label={t("nav.doctors")} stat={data.doctors} />
              <UsageBar label={t("nav.employees")} stat={data.employees} />
              <UsageBar label={t("nav.patients")} stat={data.patients} />
              <UsageBar label={t("subscription.appointmentsThisMonth")} stat={data.appointmentsThisMonth} />
            </div>

            {canManage && data.subscription.autoRenew && (
              <button className={`${secondaryButtonClass} mt-5`} onClick={handleCancel} disabled={isCancelling}>
                {isCancelling ? t("subscription.cancelling") : t("subscription.cancelAutoRenewal")}
              </button>
            )}
          </div>
        )}

        {canManage && plans && plans.length > 0 && (
          <div>
            <p className="mb-3 text-sm font-medium text-ink">{t("subscription.availablePlans")}</p>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
              {plans.map((plan) => {
                const isCurrent = data?.subscription.planId === plan.id;
                return (
                  <div key={plan.id} className="flex flex-col rounded-lg border border-line bg-surface p-5 shadow-card">
                    <p className="font-medium text-ink">{plan.name}</p>
                    <p className="text-xs text-ink-faint">{translatePlanTier(plan.tier, t)}</p>
                    <p className="mt-2 font-display text-2xl text-ink">
                      {formatCurrency(plan.price)}
                      <span className="font-sans text-xs font-normal text-ink-faint">
                        {" "}
                        / {t("subscription.durationDays", { count: plan.durationInDays })}
                      </span>
                    </p>
                    <ul className="mt-3 flex-1 space-y-1 text-xs text-ink-muted">
                      <li>{t("subscription.maxDoctors", { count: plan.maxDoctors })}</li>
                      <li>{t("subscription.maxEmployees", { count: plan.maxEmployees })}</li>
                      <li>{t("subscription.maxPatients", { count: plan.maxPatients })}</li>
                      <li>{t("subscription.maxAppointments", { count: plan.maxAppointmentsPerMonth })}</li>
                    </ul>
                    <button
                      className={`${isCurrent ? secondaryButtonClass : primaryButtonClass} mt-4`}
                      onClick={() => handleUpgrade(plan)}
                      disabled={isCurrent || busyPlanId === plan.id}
                    >
                      {isCurrent
                        ? t("subscription.currentPlan")
                        : busyPlanId === plan.id
                          ? plan.price > 0
                            ? t("subscription.redirectingToPayment")
                            : t("subscription.switching")
                          : plan.price > 0
                            ? t("subscription.payAndSwitch")
                            : t("subscription.switchToPlan")}
                    </button>
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </div>
    </AppShell>
  );
}
