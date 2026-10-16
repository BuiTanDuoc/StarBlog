import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { Pagination } from "../../components/Pagination";
import { AppointmentFormModal } from "./AppointmentFormModal";
import { CancelAppointmentModal } from "./CancelAppointmentModal";
import { RescheduleAppointmentModal } from "./RescheduleAppointmentModal";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import { api, ApiError } from "../../lib/api";
import type { PaginatedList } from "../../lib/types";
import {
  APPOINTMENT_STATUS_STYLES,
  translateAppointmentStatus,
  formatDate,
  formatTime,
} from "../../lib/format";
import { inputClass, primaryButtonClass } from "../../lib/ui";

interface AppointmentListItem {
  id: string;
  patientId: string;
  patientName: string;
  doctorId: string;
  doctorName: string;
  branchId: string;
  branchName: string;
  scheduledStart: string;
  scheduledEnd: string;
  status: string;
  notes?: string | null;
}

const PAGE_SIZE = 20;
const TERMINAL_STATUSES = ["Cancelled", "Completed", "NoShow"];

export function AppointmentsPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];
  const canCreate = roles.some((r) => ["ClinicOwner", "Receptionist", "SuperAdmin"].includes(r));
  const canEdit = roles.some((r) => ["ClinicOwner", "Doctor", "Receptionist", "SuperAdmin"].includes(r));

  const [pageNumber, setPageNumber] = useState(1);
  const [statusFilter, setStatusFilter] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [showCreate, setShowCreate] = useState(false);
  const [cancelTarget, setCancelTarget] = useState<AppointmentListItem | null>(null);
  const [rescheduleTarget, setRescheduleTarget] = useState<AppointmentListItem | null>(null);
  const [statusUpdateError, setStatusUpdateError] = useState<string | null>(null);

  useEffect(() => setPageNumber(1), [statusFilter, fromDate, toDate]);

  const query = new URLSearchParams({
    pageNumber: String(pageNumber),
    pageSize: String(PAGE_SIZE),
    ...(statusFilter ? { status: statusFilter } : {}),
    ...(fromDate ? { fromDate } : {}),
    ...(toDate ? { toDate } : {}),
  });

  const { data, isLoading, error, refetch } = useApiData<PaginatedList<AppointmentListItem>>(
    `/api/appointments?${query.toString()}`,
  );

  async function handleStatusChange(appointment: AppointmentListItem, newStatus: string) {
    setStatusUpdateError(null);
    try {
      await api.put(`/api/appointments/${appointment.id}/status`, { newStatus, reason: null });
      refetch();
    } catch (err) {
      setStatusUpdateError(err instanceof ApiError ? err.message : t("appointments.failedToUpdateStatus"));
    }
  }

  return (
    <AppShell title={t("nav.appointments")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="flex flex-wrap items-center gap-3">
            <select className={`${inputClass} w-auto`} value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
              <option value="">{t("appointments.allStatuses")}</option>
              {Object.keys(APPOINTMENT_STATUS_STYLES).map((value) => (
                <option key={value} value={value}>
                  {translateAppointmentStatus(value, t)}
                </option>
              ))}
            </select>
            <input
              type="date"
              className={`${inputClass} w-auto`}
              value={fromDate}
              onChange={(e) => setFromDate(e.target.value)}
              aria-label={t("appointments.fromDate")}
            />
            <span className="text-xs text-ink-faint">{t("appointments.to")}</span>
            <input
              type="date"
              className={`${inputClass} w-auto`}
              value={toDate}
              onChange={(e) => setToDate(e.target.value)}
              aria-label={t("appointments.toDate")}
            />
          </div>
          {canCreate && (
            <button className={primaryButtonClass} onClick={() => setShowCreate(true)}>
              + {t("appointments.newAppointment")}
            </button>
          )}
        </div>

        {statusUpdateError && <p className="text-sm text-danger">{statusUpdateError}</p>}

        <div className="overflow-hidden rounded-lg border border-line bg-surface shadow-card">
          {isLoading && (
            <p className="px-5 py-8 text-center font-mono text-sm text-ink-muted">{t("appointments.loadingAppointments")}</p>
          )}
          {error && <p className="px-5 py-4 text-sm text-danger">{error}</p>}

          {data && data.items.length === 0 && !isLoading && (
            <p className="px-5 py-10 text-center text-sm text-ink-faint">{t("appointments.noMatch")}</p>
          )}

          {data && data.items.length > 0 && (
            <table className="w-full text-start text-sm">
              <thead>
                <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                  <th className="px-5 py-3">{t("appointments.patient")}</th>
                  <th className="px-5 py-3">{t("appointments.doctor")}</th>
                  <th className="px-5 py-3">{t("appointments.branch")}</th>
                  <th className="px-5 py-3">{t("appointments.when")}</th>
                  <th className="px-5 py-3">{t("common.status")}</th>
                  {canEdit && <th className="px-5 py-3 text-end">{t("common.actions")}</th>}
                </tr>
              </thead>
              <tbody>
                {data.items.map((a) => {
                  const isTerminal = TERMINAL_STATUSES.includes(a.status);
                  return (
                    <tr key={a.id} className="border-b border-line last:border-0 hover:bg-canvas/60">
                      <td className="px-5 py-3 font-medium text-ink">{a.patientName}</td>
                      <td className="px-5 py-3 text-ink-muted">{a.doctorName}</td>
                      <td className="px-5 py-3 text-ink-faint">{a.branchName}</td>
                      <td className="px-5 py-3 text-ink-muted">
                        {formatDate(a.scheduledStart)}
                        <span className="ms-1.5 font-mono text-xs text-ink-faint">
                          {formatTime(a.scheduledStart)}–{formatTime(a.scheduledEnd)}
                        </span>
                      </td>
                      <td className="px-5 py-3">
                        {canEdit && !isTerminal ? (
                          <select
                            className={`rounded-full border-0 px-2.5 py-1 text-xs font-medium ${APPOINTMENT_STATUS_STYLES[a.status] ?? "bg-line text-ink-muted"}`}
                            value={a.status}
                            onChange={(e) => handleStatusChange(a, e.target.value)}
                          >
                            {Object.keys(APPOINTMENT_STATUS_STYLES)
                              .filter((value) => !TERMINAL_STATUSES.includes(value) || value === a.status)
                              .map((value) => (
                                <option key={value} value={value}>
                                  {translateAppointmentStatus(value, t)}
                                </option>
                              ))}
                          </select>
                        ) : (
                          <span
                            className={`rounded-full px-2.5 py-1 text-xs font-medium ${APPOINTMENT_STATUS_STYLES[a.status] ?? "bg-line text-ink-muted"}`}
                          >
                            {translateAppointmentStatus(a.status, t)}
                          </span>
                        )}
                      </td>
                      {canEdit && (
                        <td className="px-5 py-3 text-end">
                          {!isTerminal && (
                            <>
                              <button
                                className="text-xs font-medium text-harbor-500 hover:underline"
                                onClick={() => setRescheduleTarget(a)}
                              >
                                {t("appointments.reschedule")}
                              </button>
                              <button
                                className="ms-3 text-xs font-medium text-danger hover:underline"
                                onClick={() => setCancelTarget(a)}
                              >
                                {t("common.cancel")}
                              </button>
                            </>
                          )}
                        </td>
                      )}
                    </tr>
                  );
                })}
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

      {showCreate && (
        <AppointmentFormModal
          onClose={() => setShowCreate(false)}
          onSaved={() => {
            setShowCreate(false);
            refetch();
          }}
        />
      )}

      {cancelTarget && (
        <CancelAppointmentModal
          appointmentId={cancelTarget.id}
          patientName={cancelTarget.patientName}
          onClose={() => setCancelTarget(null)}
          onCancelled={() => {
            setCancelTarget(null);
            refetch();
          }}
        />
      )}

      {rescheduleTarget && (
        <RescheduleAppointmentModal
          appointmentId={rescheduleTarget.id}
          doctorId={rescheduleTarget.doctorId}
          patientName={rescheduleTarget.patientName}
          currentStart={rescheduleTarget.scheduledStart}
          onClose={() => setRescheduleTarget(null)}
          onRescheduled={() => {
            setRescheduleTarget(null);
            refetch();
          }}
        />
      )}
    </AppShell>
  );
}
