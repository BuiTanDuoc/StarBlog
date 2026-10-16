import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { BranchFormModal, type BranchDto } from "./BranchFormModal";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import { api, ApiError } from "../../lib/api";
import { primaryButtonClass } from "../../lib/ui";

export function BranchesPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];
  const canManage = roles.some((r) => ["ClinicOwner", "SuperAdmin"].includes(r));

  const [formState, setFormState] = useState<{ mode: "create" } | { mode: "edit"; branch: BranchDto } | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<BranchDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const { data, isLoading, error, refetch } = useApiData<BranchDto[]>("/api/branches");

  async function handleDelete() {
    if (!deleteTarget) return;
    setIsDeleting(true);
    setDeleteError(null);
    try {
      await api.delete(`/api/branches/${deleteTarget.id}`);
      setDeleteTarget(null);
      refetch();
    } catch (err) {
      setDeleteError(err instanceof ApiError ? err.message : t("branches.failedToDelete"));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <AppShell title={t("nav.branches")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <p className="text-sm text-ink-muted">{t("branches.subtitle")}</p>
          {canManage && (
            <button className={primaryButtonClass} onClick={() => setFormState({ mode: "create" })}>
              + {t("branches.addBranch")}
            </button>
          )}
        </div>

        <div className="overflow-hidden rounded-lg border border-line bg-surface shadow-card">
          {isLoading && <p className="px-5 py-8 text-center font-mono text-sm text-ink-muted">{t("branches.loadingBranches")}</p>}
          {error && <p className="px-5 py-4 text-sm text-danger">{error}</p>}

          {data && data.length === 0 && !isLoading && (
            <p className="px-5 py-10 text-center text-sm text-ink-faint">{t("branches.noBranchesYet")}</p>
          )}

          {data && data.length > 0 && (
            <table className="w-full text-start text-sm">
              <thead>
                <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                  <th className="px-5 py-3">{t("common.name")}</th>
                  <th className="px-5 py-3">{t("common.address")}</th>
                  <th className="px-5 py-3">{t("common.phone")}</th>
                  <th className="px-5 py-3">{t("common.email")}</th>
                  <th className="px-5 py-3">{t("common.status")}</th>
                  {canManage && <th className="px-5 py-3 text-end">{t("common.actions")}</th>}
                </tr>
              </thead>
              <tbody>
                {data.map((b) => (
                  <tr key={b.id} className="border-b border-line last:border-0 hover:bg-canvas/60">
                    <td className="px-5 py-3 font-medium text-ink">{b.name}</td>
                    <td className="px-5 py-3 text-ink-muted">{b.address}</td>
                    <td className="px-5 py-3 text-ink-muted">{b.phone}</td>
                    <td className="px-5 py-3 text-ink-muted">{b.email || "—"}</td>
                    <td className="px-5 py-3">
                      <span
                        className={`rounded-full px-2.5 py-1 text-xs font-medium ${
                          b.isActive ? "bg-success-light text-success" : "bg-line text-ink-muted"
                        }`}
                      >
                        {b.isActive ? t("common.active") : t("common.inactive")}
                      </span>
                    </td>
                    {canManage && (
                      <td className="px-5 py-3 text-end">
                        <button
                          className="text-xs font-medium text-harbor-500 hover:underline"
                          onClick={() => setFormState({ mode: "edit", branch: b })}
                        >
                          {t("common.edit")}
                        </button>
                        <button
                          className="ms-3 text-xs font-medium text-danger hover:underline"
                          onClick={() => setDeleteTarget(b)}
                        >
                          {t("common.delete")}
                        </button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {formState && (
        <BranchFormModal
          branch={formState.mode === "edit" ? formState.branch : undefined}
          onClose={() => setFormState(null)}
          onSaved={() => {
            setFormState(null);
            refetch();
          }}
        />
      )}

      {deleteTarget && (
        <ConfirmDialog
          title={t("branches.deleteBranch")}
          message={t("branches.deleteConfirmMessage", { name: deleteTarget.name })}
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
