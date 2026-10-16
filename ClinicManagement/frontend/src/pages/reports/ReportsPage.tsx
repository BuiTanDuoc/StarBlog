import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { useAuth } from "../../lib/auth";
import { ApiError, downloadFile } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass } from "../../lib/ui";

interface ReportDef {
  key: string;
  path: string;
  fileBaseName: string;
  hasDateRange: boolean;
}

const BASE_REPORTS: ReportDef[] = [
  { key: "patients", path: "/api/reports/patients", fileBaseName: "Patients-report", hasDateRange: true },
  { key: "appointments", path: "/api/reports/appointments", fileBaseName: "Appointments-report", hasDateRange: true },
  { key: "revenue", path: "/api/reports/revenue", fileBaseName: "Revenue-report", hasDateRange: true },
  {
    key: "doctor-performance",
    path: "/api/reports/doctor-performance",
    fileBaseName: "Doctor-performance-report",
    hasDateRange: true,
  },
];

const SUBSCRIPTION_REPORT: ReportDef = {
  key: "subscriptions",
  path: "/api/reports/subscriptions",
  fileBaseName: "Subscription-report",
  hasDateRange: false,
};

export function ReportsPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];
  const isSuperAdmin = roles.includes("SuperAdmin");
  const reports = isSuperAdmin ? [...BASE_REPORTS, SUBSCRIPTION_REPORT] : BASE_REPORTS;

  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [format, setFormat] = useState<"excel" | "pdf">("excel");
  const [downloadingKey, setDownloadingKey] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function handleDownload(report: ReportDef) {
    setDownloadingKey(report.key);
    setError(null);
    try {
      const params = new URLSearchParams({ format });
      if (report.hasDateRange) {
        if (from) params.set("from", new Date(from).toISOString());
        if (to) params.set("to", new Date(to).toISOString());
      }
      const ext = format === "pdf" ? "pdf" : "xlsx";
      await downloadFile(`${report.path}?${params.toString()}`, `${report.fileBaseName}.${ext}`);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("reports.failedToDownload"));
    } finally {
      setDownloadingKey(null);
    }
  }

  return (
    <AppShell title={t("nav.reports")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-5">
        <div className="flex flex-wrap items-end gap-4 rounded-lg border border-line bg-surface p-4 shadow-card">
          <div>
            <label className={labelClass}>{t("reports.fromOptional")}</label>
            <input type="date" className={inputClass} value={from} onChange={(e) => setFrom(e.target.value)} />
          </div>
          <div>
            <label className={labelClass}>{t("reports.toOptional")}</label>
            <input type="date" className={inputClass} value={to} onChange={(e) => setTo(e.target.value)} />
          </div>
          <div>
            <label className={labelClass}>{t("reports.format")}</label>
            <select
              className={inputClass}
              value={format}
              onChange={(e) => setFormat(e.target.value as "excel" | "pdf")}
            >
              <option value="excel">{t("reports.excel")}</option>
              <option value="pdf">{t("reports.pdf")}</option>
            </select>
          </div>
          <p className="pb-2 text-xs text-ink-faint">{t("reports.dateRangeHint")}</p>
        </div>

        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          {reports.map((report) => (
            <div key={report.key} className="rounded-lg border border-line bg-surface p-5 shadow-card">
              <p className="font-medium text-ink">{t(`reports.titles.${report.key}`)}</p>
              <p className="mt-1 text-sm text-ink-muted">{t(`reports.descriptions.${report.key}`)}</p>
              <button
                className={`${primaryButtonClass} mt-4`}
                onClick={() => handleDownload(report)}
                disabled={downloadingKey === report.key}
              >
                {downloadingKey === report.key
                  ? t("reports.downloading")
                  : t("reports.downloadFormat", { format: format === "pdf" ? t("reports.pdf") : t("reports.excel") })}
              </button>
            </div>
          ))}
        </div>
      </div>
    </AppShell>
  );
}
