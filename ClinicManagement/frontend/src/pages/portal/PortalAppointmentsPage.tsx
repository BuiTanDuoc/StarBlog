import { useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { PortalShell } from "./PortalShell";
import { useApiData } from "../../lib/useApiData";
import { api, ApiError } from "../../lib/api";
import type { PaginatedList } from "../../lib/types";
import { formatDate, formatTime, translateAppointmentStatus, APPOINTMENT_STATUS_STYLES } from "../../lib/format";
import { inputClass, primaryButtonClass, secondaryButtonClass, dangerButtonClass } from "../../lib/ui";

interface PortalAppointment {
  id: string;
  doctorName: string;
  branchName: string;
  scheduledStart: string;
  scheduledEnd: string;
  status: string;
}

const CANCELLABLE_STATUSES = ["Pending", "Confirmed"];

export function PortalAppointmentsPage() {
  const { t } = useTranslation();
  const { data, isLoading, refetch } = useApiData<PaginatedList<PortalAppointment>>(
    "/api/patient-portal/appointments?pageNumber=1&pageSize=50",
  );
  const [cancelTarget, setCancelTarget] = useState<PortalAppointment | null>(null);
  const [reason, setReason] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isCancelling, setIsCancelling] = useState(false);

  async function handleCancel() {
    if (!cancelTarget) return;
    setError(null);
    setIsCancelling(true);
    try {
      await api.put(`/api/patient-portal/appointments/${cancelTarget.id}/cancel`, { reason });
      setCancelTarget(null);
      setReason("");
      refetch();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("portal.couldNotCancel"));
    } finally {
      setIsCancelling(false);
    }
  }

  const appointments = (data?.items ?? []).sort(
    (a, b) => new Date(b.scheduledStart).getTime() - new Date(a.scheduledStart).getTime(),
  );

  return (
    <PortalShell title={t("portal.myAppointments")}>
      <div className="mb-5 flex justify-end">
        <Link to="/portal/book" className={primaryButtonClass}>
          {t("portal.bookAppointment")}
        </Link>
      </div>

      <div className="overflow-hidden rounded-sm border border-line bg-surface">
        <table className="w-full text-start text-sm">
          <thead className="border-b border-line text-xs uppercase tracking-[0.06em] text-ink-faint">
            <tr>
              <th className="px-5 py-3">{t("appointments.doctor")}</th>
              <th className="px-5 py-3">{t("appointments.branch")}</th>
              <th className="px-5 py-3">{t("appointments.when")}</th>
              <th className="px-5 py-3">{t("common.status")}</th>
              <th className="px-5 py-3" />
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr><td colSpan={5} className="px-5 py-6 text-center text-ink-muted">{t("common.loading")}</td></tr>
            )}
            {!isLoading && appointments.length === 0 && (
              <tr><td colSpan={5} className="px-5 py-6 text-center text-ink-muted">{t("portal.noAppointmentsYet")}</td></tr>
            )}
            {appointments.map((a) => (
              <tr key={a.id} className="border-b border-line last:border-0">
                <td className="px-5 py-3 font-medium text-ink">{a.doctorName}</td>
                <td className="px-5 py-3 text-ink-muted">{a.branchName}</td>
                <td className="px-5 py-3 text-ink-muted">
                  {formatDate(a.scheduledStart)} · {formatTime(a.scheduledStart)}
                </td>
                <td className="px-5 py-3">
                  <span className={`rounded-sm px-2 py-0.5 text-xs ${APPOINTMENT_STATUS_STYLES[a.status] ?? ""}`}>
                    {translateAppointmentStatus(a.status, t)}
                  </span>
                </td>
                <td className="px-5 py-3 text-end">
                  {CANCELLABLE_STATUSES.includes(a.status) && (
                    <button
                      onClick={() => setCancelTarget(a)}
                      className="text-xs font-medium text-danger hover:underline"
                    >
                      {t("common.cancel")}
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {cancelTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
          <div className="w-full max-w-sm rounded-sm border border-line bg-surface p-6">
            <h2 className="font-display text-lg text-ink">{t("appointments.cancelAppointment")}</h2>
            <p className="mt-1.5 text-sm text-ink-muted">
              {t("portal.cancelWithDoctorOn", { doctor: cancelTarget.doctorName, date: formatDate(cancelTarget.scheduledStart) })}
            </p>
            <textarea
              className={`${inputClass} mt-4`}
              rows={3}
              placeholder={t("portal.reasonForCancelling")}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
            />
            {error && <p role="alert" className="mt-2 text-sm text-danger">{error}</p>}
            <div className="mt-4 flex justify-end gap-2">
              <button
                className={secondaryButtonClass}
                onClick={() => {
                  setCancelTarget(null);
                  setError(null);
                }}
              >
                {t("portal.neverMind")}
              </button>
              <button className={dangerButtonClass} disabled={!reason || isCancelling} onClick={handleCancel}>
                {isCancelling ? t("appointments.cancelling") : t("portal.confirmCancellation")}
              </button>
            </div>
          </div>
        </div>
      )}
    </PortalShell>
  );
}
