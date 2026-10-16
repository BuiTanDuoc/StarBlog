import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { Pagination } from "../../components/Pagination";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { EmployeeFormModal } from "./EmployeeFormModal";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import { api, ApiError } from "../../lib/api";
import type { PaginatedList } from "../../lib/types";
import { EMPLOYEE_STATUS_STYLES, translateEmployeeStatus } from "../../lib/format";
import { primaryButtonClass } from "../../lib/ui";

interface EmployeeListItem {
  id: string;
  fullName: string;
  position: string;
  status: string;
}

const PAGE_SIZE = 20;

export function EmployeesPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];

  const [pageNumber, setPageNumber] = useState(1);
  const [formState, setFormState] = useState<{ mode: "create" } | { mode: "edit"; id: string } | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<EmployeeListItem | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const query = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: String(PAGE_SIZE) });
  const { data, isLoading, error, refetch } = useApiData<PaginatedList<EmployeeListItem>>(
    `/api/employees?${query.toString()}`,
  );

  async function handleDelete() {
    if (!deleteTarget) return;
    setIsDeleting(true);
    setDeleteError(null);
    try {
      await api.delete(`/api/employees/${deleteTarget.id}`);
      setDeleteTarget(null);
      refetch();
    } catch (err) {
      setDeleteError(err instanceof ApiError ? err.message : t("employees.failedToDelete"));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <AppShell title={t("nav.employees")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <p className="text-sm text-ink-muted">{t("employees.subtitle")}</p>
          <button className={primaryButtonClass} onClick={() => setFormState({ mode: "create" })}>
            + {t("employees.addEmployee")}
          </button>
        </div>

        <div className="overflow-hidden rounded-lg border border-line bg-surface shadow-card">
          {isLoading && <p className="px-5 py-8 text-center font-mono text-sm text-ink-muted">{t("employees.loadingEmployees")}</p>}
          {error && <p className="px-5 py-4 text-sm text-danger">{error}</p>}

          {data && data.items.length === 0 && !isLoading && (
            <p className="px-5 py-10 text-center text-sm text-ink-faint">{t("employees.noEmployeesYet")}</p>
          )}

          {data && data.items.length > 0 && (
            <table className="w-full text-start text-sm">
              <thead>
                <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                  <th className="px-5 py-3">{t("common.name")}</th>
                  <th className="px-5 py-3">{t("employees.position")}</th>
                  <th className="px-5 py-3">{t("common.status")}</th>
                  <th className="px-5 py-3 text-end">{t("common.actions")}</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((e) => (
                  <tr key={e.id} className="border-b border-line last:border-0 hover:bg-canvas/60">
                    <td className="px-5 py-3 font-medium text-ink">{e.fullName}</td>
                    <td className="px-5 py-3 text-ink-muted">{e.position}</td>
                    <td className="px-5 py-3">
                      <span
                        className={`rounded-full px-2.5 py-1 text-xs font-medium ${EMPLOYEE_STATUS_STYLES[e.status] ?? "bg-line text-ink-muted"}`}
                      >
                        {translateEmployeeStatus(e.status, t)}
                      </span>
                    </td>
                    <td className="px-5 py-3 text-end">
                      <button
                        className="text-xs font-medium text-harbor-500 hover:underline"
                        onClick={() => setFormState({ mode: "edit", id: e.id })}
                      >
                        {t("common.edit")}
                      </button>
                      <button
                        className="ms-3 text-xs font-medium text-danger hover:underline"
                        onClick={() => setDeleteTarget(e)}
                      >
                        {t("common.delete")}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {data && (
            <Pagination
              pageNumber={data.pageNumber}
              totalPages={data.totalPages}
              totalCount={data.totalCount}
              hasPreviousPage={data.hasPreviousPage}
              hasNextPage={data.hasNextPage}
              onPageChange={setPageNumber}
            />
          )}
        </div>
      </div>

      {formState && (
        <EmployeeFormModal
          employeeId={formState.mode === "edit" ? formState.id : undefined}
          onClose={() => setFormState(null)}
          onSaved={() => {
            setFormState(null);
            refetch();
          }}
        />
      )}

      {deleteTarget && (
        <ConfirmDialog
          title={t("employees.deleteEmployee")}
          message={t("employees.deleteConfirmMessage", { name: deleteTarget.fullName })}
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
