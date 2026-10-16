import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { PortalShell } from "./PortalShell";
import { useApiData } from "../../lib/useApiData";
import type { PaginatedList } from "../../lib/types";
import { formatDate, formatTime, translateAppointmentStatus, APPOINTMENT_STATUS_STYLES } from "../../lib/format";
import { primaryButtonClass } from "../../lib/ui";

interface PortalAppointment {
  id: string;
  doctorName: string;
  branchName: string;
  scheduledStart: string;
  scheduledEnd: string;
  status: string;
}

export function PortalOverviewPage() {
  const { t } = useTranslation();
  const { data, isLoading } = useApiData<PaginatedList<PortalAppointment>>(
    "/api/patient-portal/appointments?pageNumber=1&pageSize=5",
  );

  const upcoming = (data?.items ?? []).filter((a) => new Date(a.scheduledStart) >= new Date());

  return (
    <PortalShell title={t("nav.overview")}>
      <div className="flex items-center justify-between">
        <p className="text-sm text-ink-muted">{t("portal.overviewSubtitle")}</p>
        <Link to="/portal/book" className={primaryButtonClass}>
          {t("portal.bookAppointment")}
        </Link>
      </div>

      <div className="mt-6 rounded-sm border border-line bg-surface">
        {isLoading && <p className="px-5 py-6 text-sm text-ink-muted">{t("common.loading")}</p>}
        {!isLoading && upcoming.length === 0 && (
          <p className="px-5 py-6 text-sm text-ink-muted">{t("dashboard.noUpcomingAppointments")}</p>
        )}
        {upcoming.map((a) => (
          <div key={a.id} className="flex items-center justify-between border-b border-line px-5 py-4 last:border-0">
            <div>
              <p className="text-sm font-medium text-ink">{a.doctorName}</p>
              <p className="text-xs text-ink-faint">{a.branchName}</p>
            </div>
            <div className="text-end">
              <p className="text-sm text-ink">
                {formatDate(a.scheduledStart)} · {formatTime(a.scheduledStart)}
              </p>
              <span className={`mt-1 inline-block rounded-sm px-2 py-0.5 text-xs ${APPOINTMENT_STATUS_STYLES[a.status] ?? ""}`}>
                {translateAppointmentStatus(a.status, t)}
              </span>
            </div>
          </div>
        ))}
      </div>
    </PortalShell>
  );
}
