import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { PlanFormModal, type PlanDto } from "./PlanFormModal";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import { translatePlanTier, formatCurrency } from "../../lib/format";
import { primaryButtonClass } from "../../lib/ui";

export function PlansPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];

  const [formState, setFormState] = useState<{ mode: "create" } | { mode: "edit"; plan: PlanDto } | null>(null);

  const { data, isLoading, error, refetch } = useApiData<PlanDto[]>("/api/plans?activeOnly=false");

  return (
    <AppShell title={t("nav.plans")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <p className="text-sm text-ink-muted">{t("plans.subtitle")}</p>
          <button className={primaryButtonClass} onClick={() => setFormState({ mode: "create" })}>
            + {t("plans.newPlan")}
          </button>
        </div>

        <div className="overflow-hidden rounded-lg border border-line bg-surface shadow-card">
          {isLoading && <p className="px-5 py-8 text-center font-mono text-sm text-ink-muted">{t("plans.loadingPlans")}</p>}
          {error && <p className="px-5 py-4 text-sm text-danger">{error}</p>}

          {data && data.length === 0 && !isLoading && (
            <p className="px-5 py-10 text-center text-sm text-ink-faint">{t("plans.noPlansYet")}</p>
          )}

          {data && data.length > 0 && (
            <table className="w-full text-start text-sm">
              <thead>
                <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                  <th className="px-5 py-3">{t("common.name")}</th>
                  <th className="px-5 py-3">{t("plans.tier")}</th>
                  <th className="px-5 py-3">{t("treatmentPackages.price")}</th>
                  <th className="px-5 py-3">{t("plans.duration")}</th>
                  <th className="px-5 py-3">{t("plans.limits")}</th>
                  <th className="px-5 py-3">{t("common.status")}</th>
                  <th className="px-5 py-3 text-end">{t("common.actions")}</th>
                </tr>
              </thead>
              <tbody>
                {data.map((p) => (
                  <tr key={p.id} className="border-b border-line last:border-0 hover:bg-canvas/60">
                    <td className="px-5 py-3 font-medium text-ink">{p.name}</td>
                    <td className="px-5 py-3 text-ink-muted">{translatePlanTier(p.tier, t)}</td>
                    <td className="px-5 py-3 tabular-nums text-ink-muted">{formatCurrency(p.price)}</td>
                    <td className="px-5 py-3 text-ink-muted">{t("subscription.durationDays", { count: p.durationInDays })}</td>
                    <td className="px-5 py-3 text-xs text-ink-faint">
                      {t("plans.limitsSummary", {
                        doctors: p.maxDoctors,
                        employees: p.maxEmployees,
                        patients: p.maxPatients,
                        appointments: p.maxAppointmentsPerMonth,
                      })}
                    </td>
                    <td className="px-5 py-3">
                      <span
                        className={`rounded-full px-2.5 py-1 text-xs font-medium ${
                          p.isActive ? "bg-success-light text-success" : "bg-line text-ink-muted"
                        }`}
                      >
                        {p.isActive ? t("common.active") : t("common.inactive")}
                      </span>
                    </td>
                    <td className="px-5 py-3 text-right">
                      <button
                        className="text-xs font-medium text-harbor-500 hover:underline"
                        onClick={() => setFormState({ mode: "edit", plan: p })}
                      >
                        {t("common.edit")}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {formState && (
        <PlanFormModal
          plan={formState.mode === "edit" ? formState.plan : undefined}
          onClose={() => setFormState(null)}
          onSaved={() => {
            setFormState(null);
            refetch();
          }}
        />
      )}
    </AppShell>
  );
}
