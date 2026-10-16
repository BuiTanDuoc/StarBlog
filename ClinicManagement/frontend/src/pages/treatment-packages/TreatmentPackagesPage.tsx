import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { TreatmentPackageFormModal, type TreatmentPackageDto } from "./TreatmentPackageFormModal";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import { api, ApiError } from "../../lib/api";
import { formatCurrency } from "../../lib/format";
import { primaryButtonClass } from "../../lib/ui";

export function TreatmentPackagesPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];

  const [formState, setFormState] = useState<
    { mode: "create" } | { mode: "edit"; package: TreatmentPackageDto } | null
  >(null);
  const [deleteTarget, setDeleteTarget] = useState<TreatmentPackageDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const { data, isLoading, error, refetch } = useApiData<TreatmentPackageDto[]>(
    "/api/treatmentpackages?activeOnly=false",
  );

  async function handleDelete() {
    if (!deleteTarget) return;
    setIsDeleting(true);
    setDeleteError(null);
    try {
      await api.delete(`/api/treatmentpackages/${deleteTarget.id}`);
      setDeleteTarget(null);
      refetch();
    } catch (err) {
      setDeleteError(err instanceof ApiError ? err.message : t("treatmentPackages.failedToDelete"));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <AppShell title={t("nav.treatmentPackages")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <p className="text-sm text-ink-muted">
            {t("treatmentPackages.subtitle")}
          </p>
          <button className={primaryButtonClass} onClick={() => setFormState({ mode: "create" })}>
            + {t("treatmentPackages.newPackage")}
          </button>
        </div>

        <div className="overflow-hidden rounded-lg border border-line bg-surface shadow-card">
          {isLoading && <p className="px-5 py-8 text-center font-mono text-sm text-ink-muted">{t("common.loading")}</p>}
          {error && <p className="px-5 py-4 text-sm text-danger">{error}</p>}
          {data && data.length === 0 && !isLoading && (
            <p className="px-5 py-10 text-center text-sm text-ink-faint">{t("treatmentPackages.noPackagesYetCatalog")}</p>
          )}

          {data && data.length > 0 && (
            <table className="w-full text-start text-sm">
              <thead>
                <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                  <th className="px-5 py-3">{t("common.name")}</th>
                  <th className="px-5 py-3">{t("treatmentPackages.sessions")}</th>
                  <th className="px-5 py-3">{t("treatmentPackages.price")}</th>
                  <th className="px-5 py-3">{t("treatmentPackages.validFor")}</th>
                  <th className="px-5 py-3">{t("common.status")}</th>
                  <th className="px-5 py-3 text-end">{t("common.actions")}</th>
                </tr>
              </thead>
              <tbody>
                {data.map((p) => (
                  <tr key={p.id} className="border-b border-line last:border-0 hover:bg-canvas/60">
                    <td className="px-5 py-3 font-medium text-ink">{p.name}</td>
                    <td className="px-5 py-3 text-ink-muted">{p.sessionsIncluded}</td>
                    <td className="px-5 py-3 text-ink-muted">{formatCurrency(p.price)}</td>
                    <td className="px-5 py-3 text-ink-muted">{t("treatmentPackages.daysCount", { count: p.validityDays })}</td>
                    <td className="px-5 py-3">
                      <span
                        className={`rounded-sm px-2 py-0.5 text-xs ${
                          p.isActive ? "bg-harbor-50 text-harbor-500" : "bg-canvas text-ink-faint"
                        }`}
                      >
                        {p.isActive ? t("common.active") : t("common.inactive")}
                      </span>
                    </td>
                    <td className="px-5 py-3 text-end">
                      <button
                        className="text-xs font-medium text-harbor-500 hover:underline"
                        onClick={() => setFormState({ mode: "edit", package: p })}
                      >
                        {t("common.edit")}
                      </button>
                      <button
                        className="ms-3 text-xs font-medium text-danger hover:underline"
                        onClick={() => setDeleteTarget(p)}
                      >
                        {t("common.delete")}
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
        <TreatmentPackageFormModal
          package={formState.mode === "edit" ? formState.package : undefined}
          onClose={() => setFormState(null)}
          onSaved={() => {
            setFormState(null);
            refetch();
          }}
        />
      )}

      {deleteTarget && (
        <ConfirmDialog
          title={t("treatmentPackages.deletePackage")}
          message={t("treatmentPackages.deleteConfirmMessage", { name: deleteTarget.name })}
          isBusy={isDeleting}
          onConfirm={handleDelete}
          onCancel={() => {
            setDeleteTarget(null);
            setDeleteError(null);
          }}
        />
      )}
      {deleteError && <p className="mt-2 text-sm text-danger">{deleteError}</p>}
    </AppShell>
  );
}
