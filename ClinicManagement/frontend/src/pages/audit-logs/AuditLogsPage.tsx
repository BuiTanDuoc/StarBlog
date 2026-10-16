import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { Pagination } from "../../components/Pagination";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import type { PaginatedList } from "../../lib/types";
import { AUDIT_ACTION_STYLES, translateAuditAction, formatDateTime } from "../../lib/format";
import { inputClass } from "../../lib/ui";

interface AuditLogItem {
  id: string;
  userId?: string | null;
  action: string;
  entityName: string;
  entityId: string;
  timestamp: string;
  ipAddress?: string | null;
}

const PAGE_SIZE = 50;

export function AuditLogsPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];

  const [pageNumber, setPageNumber] = useState(1);
  const [entityFilter, setEntityFilter] = useState("");
  const [actionFilter, setActionFilter] = useState("");

  const query = new URLSearchParams({
    pageNumber: String(pageNumber),
    pageSize: String(PAGE_SIZE),
    ...(entityFilter ? { entityName: entityFilter } : {}),
    ...(actionFilter ? { action: actionFilter } : {}),
  });

  const { data, isLoading, error } = useApiData<PaginatedList<AuditLogItem>>(`/api/auditlogs?${query.toString()}`);

  return (
    <AppShell title={t("nav.auditLog")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-4">
        <div className="flex flex-wrap items-center gap-3">
          <input
            className={`${inputClass} max-w-xs`}
            placeholder={t("auditLogs.filterByEntity")}
            value={entityFilter}
            onChange={(e) => {
              setEntityFilter(e.target.value);
              setPageNumber(1);
            }}
          />
          <select
            className={`${inputClass} max-w-xs`}
            value={actionFilter}
            onChange={(e) => {
              setActionFilter(e.target.value);
              setPageNumber(1);
            }}
          >
            <option value="">{t("auditLogs.allActions")}</option>
            {Object.keys(AUDIT_ACTION_STYLES).map((value) => (
              <option key={value} value={value}>
                {translateAuditAction(value, t)}
              </option>
            ))}
          </select>
        </div>

        <div className="overflow-hidden rounded-lg border border-line bg-surface shadow-card">
          {isLoading && <p className="px-5 py-8 text-center font-mono text-sm text-ink-muted">{t("auditLogs.loadingLog")}</p>}
          {error && <p className="px-5 py-4 text-sm text-danger">{error}</p>}

          {data && data.items.length === 0 && !isLoading && (
            <p className="px-5 py-10 text-center text-sm text-ink-faint">{t("auditLogs.noMatch")}</p>
          )}

          {data && data.items.length > 0 && (
            <table className="w-full text-start text-sm">
              <thead>
                <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                  <th className="px-5 py-3">{t("appointments.when")}</th>
                  <th className="px-5 py-3">{t("auditLogs.action")}</th>
                  <th className="px-5 py-3">{t("auditLogs.entity")}</th>
                  <th className="px-5 py-3">{t("auditLogs.entityId")}</th>
                  <th className="px-5 py-3">{t("auditLogs.ip")}</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((log) => (
                  <tr key={log.id} className="border-b border-line last:border-0 hover:bg-canvas/60">
                    <td className="px-5 py-3 text-ink-muted">{formatDateTime(log.timestamp)}</td>
                    <td className="px-5 py-3">
                      <span
                        className={`rounded-full px-2.5 py-1 text-xs font-medium ${AUDIT_ACTION_STYLES[log.action] ?? "bg-line text-ink-muted"}`}
                      >
                        {translateAuditAction(log.action, t)}
                      </span>
                    </td>
                    <td className="px-5 py-3 font-medium text-ink">{log.entityName}</td>
                    <td className="px-5 py-3 font-mono text-xs text-ink-faint">{log.entityId}</td>
                    <td className="px-5 py-3 font-mono text-xs text-ink-faint">{log.ipAddress || "—"}</td>
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
    </AppShell>
  );
}
