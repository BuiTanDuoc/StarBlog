import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { Pagination } from "../../components/Pagination";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { DoctorFormModal } from "./DoctorFormModal";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import { api, ApiError } from "../../lib/api";
import type { PaginatedList } from "../../lib/types";
import { DOCTOR_STATUS_STYLES, translateDoctorStatus, formatCurrency } from "../../lib/format";
import { inputClass, primaryButtonClass } from "../../lib/ui";

interface DoctorListItem {
  id: string;
  fullName: string;
  specialization: string;
  status: string;
  consultationFee: number;
}

const PAGE_SIZE = 20;

export function DoctorsPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];
  const canManage = roles.some((r) => ["ClinicOwner", "SuperAdmin"].includes(r));

  const [pageNumber, setPageNumber] = useState(1);
  const [statusFilter, setStatusFilter] = useState("");
  const [formState, setFormState] = useState<{ mode: "create" } | { mode: "edit"; id: string } | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<DoctorListItem | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const query = new URLSearchParams({
    pageNumber: String(pageNumber),
    pageSize: String(PAGE_SIZE),
    ...(statusFilter ? { status: statusFilter } : {}),
  });

  const { data, isLoading, error, refetch } = useApiData<PaginatedList<DoctorListItem>>(
    `/api/doctors?${query.toString()}`,
  );

  async function handleDelete() {
    if (!deleteTarget) return;
    setIsDeleting(true);
    setDeleteError(null);
    try {
      await api.delete(`/api/doctors/${deleteTarget.id}`);
      setDeleteTarget(null);
      refetch();
    } catch (err) {
      setDeleteError(err instanceof ApiError ? err.message : t("doctors.failedToDelete"));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <AppShell title={t("nav.doctors")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <select
            className={`${inputClass} max-w-xs`}
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value);
              setPageNumber(1);
            }}
          >
            <option value="">{t("appointments.allStatuses")}</option>
            {Object.keys(DOCTOR_STATUS_STYLES).map((value) => (
              <option key={value} value={value}>
                {translateDoctorStatus(value, t)}
              </option>
            ))}
          </select>
          {canManage && (
            <button className={primaryButtonClass} onClick={() => setFormState({ mode: "create" })}>
              + {t("doctors.addDoctor")}
            </button>
          )}
        </div>

        <div className="overflow-hidden rounded-lg border border-line bg-surface shadow-card">
          {isLoading && <p className="px-5 py-8 text-center font-mono text-sm text-ink-muted">{t("doctors.loadingDoctors")}</p>}
          {error && <p className="px-5 py-4 text-sm text-danger">{error}</p>}

          {data && data.items.length === 0 && !isLoading && (
            <p className="px-5 py-10 text-center text-sm text-ink-faint">{t("doctors.noMatch")}</p>
          )}

          {data && data.items.length > 0 && (
            <table className="w-full text-start text-sm">
              <thead>
                <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                  <th className="px-5 py-3">{t("common.name")}</th>
                  <th className="px-5 py-3">{t("doctors.specialization")}</th>
                  <th className="px-5 py-3">{t("doctors.consultationFee")}</th>
                  <th className="px-5 py-3">{t("common.status")}</th>
                  {canManage && <th className="px-5 py-3 text-end">{t("common.actions")}</th>}
                </tr>
              </thead>
              <tbody>
                {data.items.map((d) => (
                  <tr key={d.id} className="border-b border-line last:border-0 hover:bg-canvas/60">
                    <td className="px-5 py-3 font-medium text-ink">{d.fullName}</td>
                    <td className="px-5 py-3 text-ink-muted">{d.specialization}</td>
                    <td className="px-5 py-3 tabular-nums text-ink-muted">{formatCurrency(d.consultationFee)}</td>
                    <td className="px-5 py-3">
                      <span
                        className={`rounded-full px-2.5 py-1 text-xs font-medium ${DOCTOR_STATUS_STYLES[d.status] ?? "bg-line text-ink-muted"}`}
                      >
                        {translateDoctorStatus(d.status, t)}
                      </span>
                    </td>
                    {canManage && (
                      <td className="px-5 py-3 text-end">
                        <button
                          className="text-xs font-medium text-harbor-500 hover:underline"
                          onClick={() => setFormState({ mode: "edit", id: d.id })}
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
        <DoctorFormModal
          doctorId={formState.mode === "edit" ? formState.id : undefined}
          onClose={() => setFormState(null)}
          onSaved={() => {
            setFormState(null);
            refetch();
          }}
        />
      )}

      {deleteTarget && (
        <ConfirmDialog
          title={t("doctors.deleteDoctor")}
          message={t("doctors.deleteConfirmMessage", { name: deleteTarget.fullName })}
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
