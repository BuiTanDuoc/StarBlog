import { useState } from "react";
import { useTranslation } from "react-i18next";
import { PortalShell } from "./PortalShell";
import { useApiData } from "../../lib/useApiData";
import { api, ApiError } from "../../lib/api";
import type { PaginatedList } from "../../lib/types";
import { formatDate, formatCurrency, translateInvoiceStatus, INVOICE_STATUS_STYLES } from "../../lib/format";
import { primaryButtonClass } from "../../lib/ui";

interface PortalInvoice {
  id: string;
  invoiceNumber: string;
  issueDate: string;
  total: number;
  status: string;
}

const PAYABLE_STATUSES = ["Issued", "PartiallyPaid", "Overdue"];

export function PortalInvoicesPage() {
  const { t } = useTranslation();
  const { data, isLoading } = useApiData<PaginatedList<PortalInvoice>>(
    "/api/patient-portal/invoices?pageNumber=1&pageSize=50",
  );
  const [payingId, setPayingId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const invoices = [...(data?.items ?? [])].sort(
    (a, b) => new Date(b.issueDate).getTime() - new Date(a.issueDate).getTime(),
  );

  async function handlePayNow(invoiceId: string) {
    setError(null);
    setPayingId(invoiceId);
    try {
      const origin = window.location.origin;
      const response = await api.post<{ checkoutUrl: string }>(`/api/patient-portal/invoices/${invoiceId}/checkout`, {
        successUrl: `${origin}/billing/checkout-success?back=/portal/invoices`,
        cancelUrl: `${origin}/billing/checkout-cancelled?back=/portal/invoices`,
      });
      // The patient is paying their own bill right now, so redirect their own browser —
      // unlike the staff side, which generates a link to send someone else.
      window.location.href = response.checkoutUrl;
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("portal.couldNotStartCheckout"));
      setPayingId(null);
    }
  }

  return (
    <PortalShell title={t("nav.invoices")}>
      {error && <p role="alert" className="mb-4 rounded-sm bg-danger-light px-3.5 py-2.5 text-sm text-danger">{error}</p>}

      <div className="overflow-hidden rounded-sm border border-line bg-surface">
        <table className="w-full text-start text-sm">
          <thead className="border-b border-line text-xs uppercase tracking-[0.06em] text-ink-faint">
            <tr>
              <th className="px-5 py-3">{t("invoices.invoiceNumber")}</th>
              <th className="px-5 py-3">{t("common.date")}</th>
              <th className="px-5 py-3">{t("common.total")}</th>
              <th className="px-5 py-3">{t("common.status")}</th>
              <th className="px-5 py-3" />
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr><td colSpan={5} className="px-5 py-6 text-center text-ink-muted">{t("common.loading")}</td></tr>
            )}
            {!isLoading && invoices.length === 0 && (
              <tr><td colSpan={5} className="px-5 py-6 text-center text-ink-muted">{t("portal.noInvoicesYet")}</td></tr>
            )}
            {invoices.map((inv) => (
              <tr key={inv.id} className="border-b border-line last:border-0">
                <td className="px-5 py-3 font-medium text-ink">{inv.invoiceNumber}</td>
                <td className="px-5 py-3 text-ink-muted">{formatDate(inv.issueDate)}</td>
                <td className="px-5 py-3 text-ink-muted">{formatCurrency(inv.total)}</td>
                <td className="px-5 py-3">
                  <span className={`rounded-sm px-2 py-0.5 text-xs ${INVOICE_STATUS_STYLES[inv.status] ?? ""}`}>
                    {translateInvoiceStatus(inv.status, t)}
                  </span>
                </td>
                <td className="px-5 py-3 text-end">
                  {PAYABLE_STATUSES.includes(inv.status) && (
                    <button
                      className={`${primaryButtonClass} !px-3 !py-1.5 text-xs`}
                      disabled={payingId === inv.id}
                      onClick={() => handlePayNow(inv.id)}
                    >
                      {payingId === inv.id ? t("portal.redirecting") : t("portal.payNow")}
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </PortalShell>
  );
}
