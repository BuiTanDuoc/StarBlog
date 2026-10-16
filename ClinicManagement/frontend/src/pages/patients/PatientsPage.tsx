import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { Pagination } from "../../components/Pagination";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { PatientFormModal } from "./PatientFormModal";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import { api, ApiError } from "../../lib/api";
import type { PaginatedList } from "../../lib/types";
import { calculateAge, formatDate, translateGender } from "../../lib/format";
import { primaryButtonClass } from "../../lib/ui";

interface PatientListItem {
  id: string;
  patientCode: string;
  fullName: string;
  gender: "Male" | "Female" | "Other";
  dateOfBirth: string;
  phone: string;
  createdAt: string;
}

const PAGE_SIZE = 20;

export function PatientsPage() {
  const { user } = useAuth();
  const { t } = useTranslation();
  const roles = user?.roles ?? [];
  const canManage = roles.some((r) => ["ClinicOwner", "Receptionist", "SuperAdmin"].includes(r));
  const canDelete = roles.some((r) => ["ClinicOwner", "SuperAdmin"].includes(r));
  const canViewChart = roles.some((r) => ["ClinicOwner", "Doctor", "Nurse", "SuperAdmin"].includes(r));

  const [searchInput, setSearchInput] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [formState, setFormState] = useState<{ mode: "create" } | { mode: "edit"; id: string } | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<PatientListItem | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  useEffect(() => {
    const handle = setTimeout(() => {
      setSearchTerm(searchInput.trim());
      setPageNumber(1);
    }, 350);
    return () => clearTimeout(handle);
  }, [searchInput]);

  const query = new URLSearchParams({
    pageNumber: String(pageNumber),
    pageSize: String(PAGE_SIZE),
    ...(searchTerm ? { searchTerm } : {}),
  });

  const { data, isLoading, error, refetch } = useApiData<PaginatedList<PatientListItem>>(
    `/api/patients?${query.toString()}`,
  );

  async function handleDelete() {
    if (!deleteTarget) return;
    setIsDeleting(true);
    setDeleteError(null);
    try {
      await api.delete(`/api/patients/${deleteTarget.id}`);
      setDeleteTarget(null);
      refetch();
    } catch (err) {
      setDeleteError(err instanceof ApiError ? err.message : t("patients.failedToDelete"));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <AppShell title={t("nav.patients")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <input
            className="w-full max-w-xs rounded-sm border border-line bg-surface px-3 py-2 text-sm text-ink placeholder:text-ink-faint focus:border-harbor-400 focus:outline-none"
            placeholder={t("patients.searchPlaceholder")}
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
          {canManage && (
            <button className={primaryButtonClass} onClick={() => setFormState({ mode: "create" })}>
              + {t("patients.addPatient")}
            </button>
          )}
        </div>

        <div className="overflow-hidden rounded-lg border border-line bg-surface shadow-card">
          {isLoading && <p className="px-5 py-8 text-center font-mono text-sm text-ink-muted">{t("patients.loadingPatients")}</p>}
          {error && <p className="px-5 py-4 text-sm text-danger">{error}</p>}

          {data && data.items.length === 0 && !isLoading && (
            <p className="px-5 py-10 text-center text-sm text-ink-faint">
              {searchTerm ? t("patients.noMatch") : t("patients.noPatientsYet")}
            </p>
          )}

          {data && data.items.length > 0 && (
            <table className="w-full text-start text-sm">
              <thead>
                <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                  <th className="px-5 py-3">{t("patients.code")}</th>
                  <th className="px-5 py-3">{t("common.name")}</th>
                  <th className="px-5 py-3">{t("patients.gender")}</th>
                  <th className="px-5 py-3">{t("patients.age")}</th>
                  <th className="px-5 py-3">{t("common.phone")}</th>
                  <th className="px-5 py-3">{t("patients.registered")}</th>
                  {(canManage || canViewChart) && <th className="px-5 py-3 text-end">{t("common.actions")}</th>}
                </tr>
              </thead>
              <tbody>
                {data.items.map((p) => (
                  <tr key={p.id} className="border-b border-line last:border-0 hover:bg-canvas/60">
                    <td className="px-5 py-3 font-mono text-xs text-ink-faint">{p.patientCode}</td>
                    <td className="px-5 py-3 font-medium text-ink">{p.fullName}</td>
                    <td className="px-5 py-3 text-ink-muted">{translateGender(p.gender, t)}</td>
                    <td className="px-5 py-3 tabular-nums text-ink-muted">{calculateAge(p.dateOfBirth)}</td>
                    <td className="px-5 py-3 font-mono text-xs text-ink-muted">{p.phone}</td>
                    <td className="px-5 py-3 text-ink-faint">{formatDate(p.createdAt)}</td>
                    {(canManage || canViewChart) && (
                      <td className="px-5 py-3 text-end">
                        {canViewChart && (
                          <Link
                            className="text-xs font-medium text-harbor-500 hover:underline"
                            to={`/patients/${p.id}/chart`}
                          >
                            {t("patients.chart")}
                          </Link>
                        )}
                        {canManage && (
                          <button
                            className="ms-3 text-xs font-medium text-harbor-500 hover:underline"
                            onClick={() => setFormState({ mode: "edit", id: p.id })}
                          >
                            {t("common.edit")}
                          </button>
                        )}
                        {canDelete && (
                          <button
                            className="ms-3 text-xs font-medium text-danger hover:underline"
                            onClick={() => setDeleteTarget(p)}
                          >
                            {t("common.delete")}
                          </button>
                        )}
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
        <PatientFormModal
          patientId={formState.mode === "edit" ? formState.id : undefined}
          onClose={() => setFormState(null)}
          onSaved={() => {
            setFormState(null);
            refetch();
          }}
        />
      )}

      {deleteTarget && (
        <ConfirmDialog
          title={t("patients.deletePatient")}
          message={t("patients.deleteConfirmMessage", { name: deleteTarget.fullName })}
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
