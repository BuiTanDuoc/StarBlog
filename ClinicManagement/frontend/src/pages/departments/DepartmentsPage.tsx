import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { DepartmentFormModal, type DepartmentDto } from "./DepartmentFormModal";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import { api, ApiError } from "../../lib/api";
import { inputClass, primaryButtonClass } from "../../lib/ui";

interface Branch {
  id: string;
  name: string;
}

export function DepartmentsPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];
  const canManage = roles.some((r) => ["ClinicOwner", "SuperAdmin"].includes(r));

  const [branchFilter, setBranchFilter] = useState("");
  const [formState, setFormState] = useState<
    { mode: "create" } | { mode: "edit"; department: DepartmentDto } | null
  >(null);
  const [deleteTarget, setDeleteTarget] = useState<DepartmentDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const { data: branches } = useApiData<Branch[]>("/api/branches");
  const branchMap = new Map((branches ?? []).map((b) => [b.id, b.name]));

  const query = new URLSearchParams(branchFilter ? { branchId: branchFilter } : {});
  const { data, isLoading, error, refetch } = useApiData<DepartmentDto[]>(`/api/departments?${query.toString()}`);

  async function handleDelete() {
    if (!deleteTarget) return;
    setIsDeleting(true);
    setDeleteError(null);
    try {
      await api.delete(`/api/departments/${deleteTarget.id}`);
      setDeleteTarget(null);
      refetch();
    } catch (err) {
      setDeleteError(err instanceof ApiError ? err.message : t("departments.failedToDelete"));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <AppShell title={t("nav.departments")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <select className={`${inputClass} max-w-xs`} value={branchFilter} onChange={(e) => setBranchFilter(e.target.value)}>
            <option value="">{t("departments.allBranches")}</option>
            {(branches ?? []).map((b) => (
              <option key={b.id} value={b.id}>
                {b.name}
              </option>
            ))}
          </select>
          {canManage && (
            <button className={primaryButtonClass} onClick={() => setFormState({ mode: "create" })}>
              + {t("departments.addDepartment")}
            </button>
          )}
        </div>

        <div className="overflow-hidden rounded-lg border border-line bg-surface shadow-card">
          {isLoading && (
            <p className="px-5 py-8 text-center font-mono text-sm text-ink-muted">{t("departments.loadingDepartments")}</p>
          )}
          {error && <p className="px-5 py-4 text-sm text-danger">{error}</p>}

          {data && data.length === 0 && !isLoading && (
            <p className="px-5 py-10 text-center text-sm text-ink-faint">{t("departments.noMatch")}</p>
          )}

          {data && data.length > 0 && (
            <table className="w-full text-start text-sm">
              <thead>
                <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                  <th className="px-5 py-3">{t("common.name")}</th>
                  <th className="px-5 py-3">{t("appointments.branch")}</th>
                  {canManage && <th className="px-5 py-3 text-end">{t("common.actions")}</th>}
                </tr>
              </thead>
              <tbody>
                {data.map((d) => (
                  <tr key={d.id} className="border-b border-line last:border-0 hover:bg-canvas/60">
                    <td className="px-5 py-3 font-medium text-ink">{d.name}</td>
                    <td className="px-5 py-3 text-ink-muted">{branchMap.get(d.branchId) ?? "—"}</td>
                    {canManage && (
                      <td className="px-5 py-3 text-end">
                        <button
                          className="text-xs font-medium text-harbor-500 hover:underline"
                          onClick={() => setFormState({ mode: "edit", department: d })}
                        >
                          {t("common.edit")}
                        </button>
                        <button
                          className="ms-3 text-xs font-medium text-danger hover:underline"
                          onClick={() => setDeleteTarget(d)}
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
        <DepartmentFormModal
          department={formState.mode === "edit" ? formState.department : undefined}
          branches={branches ?? []}
          onClose={() => setFormState(null)}
          onSaved={() => {
            setFormState(null);
            refetch();
          }}
        />
      )}

      {deleteTarget && (
        <ConfirmDialog
          title={t("departments.deleteDepartment")}
          message={t("departments.deleteConfirmMessage", { name: deleteTarget.name })}
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
