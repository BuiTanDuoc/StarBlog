import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { Pagination } from "../../components/Pagination";
import { InvoiceFormModal } from "./InvoiceFormModal";
import { InvoiceDetailModal } from "./InvoiceDetailModal";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import type { PaginatedList } from "../../lib/types";
import { INVOICE_STATUS_STYLES, translateInvoiceStatus, formatCurrency, formatDate } from "../../lib/format";
import { inputClass, primaryButtonClass } from "../../lib/ui";

interface InvoiceListItem {
  id: string;
  invoiceNumber: string;
  patientName: string;
  issueDate: string;
  total: number;
  status: string;
}

const PAGE_SIZE = 20;

export function InvoicesPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];
  const canManageInvoices = roles.some((r) => ["ClinicOwner", "Accountant", "SuperAdmin"].includes(r));
  const canManagePayments = canManageInvoices;

  const [pageNumber, setPageNumber] = useState(1);
  const [statusFilter, setStatusFilter] = useState("");
  const [showCreate, setShowCreate] = useState(false);
  const [selectedInvoiceId, setSelectedInvoiceId] = useState<string | null>(null);

  const query = new URLSearchParams({
    pageNumber: String(pageNumber),
    pageSize: String(PAGE_SIZE),
    ...(statusFilter ? { status: statusFilter } : {}),
  });

  const { data, isLoading, error, refetch } = useApiData<PaginatedList<InvoiceListItem>>(
    `/api/invoices?${query.toString()}`,
  );

  return (
    <AppShell title={t("nav.invoices")} roleLabel={roles[0] ?? ""}>
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
            {Object.keys(INVOICE_STATUS_STYLES).map((value) => (
              <option key={value} value={value}>
                {translateInvoiceStatus(value, t)}
              </option>
            ))}
          </select>
          {canManageInvoices && (
            <button className={primaryButtonClass} onClick={() => setShowCreate(true)}>
              + {t("invoices.newInvoice")}
            </button>
          )}
        </div>

        <div className="overflow-hidden rounded-lg border border-line bg-surface shadow-card">
          {isLoading && <p className="px-5 py-8 text-center font-mono text-sm text-ink-muted">{t("invoices.loadingInvoices")}</p>}
          {error && <p className="px-5 py-4 text-sm text-danger">{error}</p>}

          {data && data.items.length === 0 && !isLoading && (
            <p className="px-5 py-10 text-center text-sm text-ink-faint">{t("invoices.noMatch")}</p>
          )}

          {data && data.items.length > 0 && (
            <table className="w-full text-start text-sm">
              <thead>
                <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                  <th className="px-5 py-3">{t("invoices.invoiceNumber")}</th>
                  <th className="px-5 py-3">{t("appointments.patient")}</th>
                  <th className="px-5 py-3">{t("invoices.issued")}</th>
                  <th className="px-5 py-3">{t("common.total")}</th>
                  <th className="px-5 py-3">{t("common.status")}</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((inv) => (
                  <tr
                    key={inv.id}
                    className="cursor-pointer border-b border-line last:border-0 hover:bg-canvas/60"
                    onClick={() => setSelectedInvoiceId(inv.id)}
                  >
                    <td className="px-5 py-3 font-mono text-xs text-ink-faint">{inv.invoiceNumber}</td>
                    <td className="px-5 py-3 font-medium text-ink">{inv.patientName}</td>
                    <td className="px-5 py-3 text-ink-muted">{formatDate(inv.issueDate)}</td>
                    <td className="px-5 py-3 tabular-nums text-ink-muted">{formatCurrency(inv.total)}</td>
                    <td className="px-5 py-3">
                      <span
                        className={`rounded-full px-2.5 py-1 text-xs font-medium ${INVOICE_STATUS_STYLES[inv.status] ?? "bg-line text-ink-muted"}`}
                      >
                        {translateInvoiceStatus(inv.status, t)}
                      </span>
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

      {showCreate && (
        <InvoiceFormModal
          onClose={() => setShowCreate(false)}
          onSaved={() => {
            setShowCreate(false);
            refetch();
          }}
        />
      )}

      {selectedInvoiceId && (
        <InvoiceDetailModal
          invoiceId={selectedInvoiceId}
          canManagePayments={canManagePayments}
          onClose={() => setSelectedInvoiceId(null)}
          onChanged={refetch}
        />
      )}
    </AppShell>
  );
}
