import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { RecordPaymentModal } from "./RecordPaymentModal";
import { useApiData } from "../../lib/useApiData";
import { api, ApiError } from "../../lib/api";
import { primaryButtonClass, secondaryButtonClass } from "../../lib/ui";
import {
  INVOICE_STATUS_STYLES,
  translateInvoiceStatus,
  translatePaymentMethod,
  formatCurrency,
  formatDate,
  formatDateTime,
} from "../../lib/format";

interface InvoiceItem {
  description: string;
  quantity: number;
  unitPrice: number;
  total: number;
}

interface Payment {
  id: string;
  amount: number;
  method: string;
  paidAt: string;
  transactionReference?: string | null;
}

interface InvoiceDetail {
  id: string;
  invoiceNumber: string;
  patientName: string;
  issueDate: string;
  dueDate: string;
  subTotal: number;
  discount: number;
  tax: number;
  total: number;
  status: string;
  amountPaid: number;
  balance: number;
  items: InvoiceItem[];
  payments: Payment[];
}

interface InvoiceDetailModalProps {
  invoiceId: string;
  canManagePayments: boolean;
  onClose: () => void;
  onChanged: () => void;
}

export function InvoiceDetailModal({ invoiceId, canManagePayments, onClose, onChanged }: InvoiceDetailModalProps) {
  const { t } = useTranslation();
  const { data, isLoading, error, refetch } = useApiData<InvoiceDetail>(`/api/invoices/${invoiceId}`);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [checkoutUrl, setCheckoutUrl] = useState<string | null>(null);
  const [checkoutError, setCheckoutError] = useState<string | null>(null);
  const [isCreatingCheckout, setIsCreatingCheckout] = useState(false);

  const canRecordPayment =
    canManagePayments && data && data.balance > 0 && !["Paid", "Void"].includes(data.status);

  async function handleCreateCheckoutLink() {
    setCheckoutError(null);
    setIsCreatingCheckout(true);
    try {
      const origin = window.location.origin;
      const response = await api.post<{ checkoutUrl: string }>(`/api/invoices/${invoiceId}/checkout`, {
        successUrl: `${origin}/billing/checkout-success`,
        cancelUrl: `${origin}/billing/checkout-cancelled`,
      });
      setCheckoutUrl(response.checkoutUrl);
    } catch (err) {
      setCheckoutError(err instanceof ApiError ? err.message : t("invoices.couldNotCreateLink"));
    } finally {
      setIsCreatingCheckout(false);
    }
  }

  return (
    <>
      <Modal
        title={data ? t("invoices.invoiceHash", { number: data.invoiceNumber }) : t("nav.invoices")}
        subtitle={data?.patientName}
        onClose={onClose}
        width="xl"
      >
        {isLoading && <p className="py-8 text-center font-mono text-sm text-ink-muted">{t("common.loading")}</p>}
        {error && <p className="text-sm text-danger">{error}</p>}

        {data && (
          <div className="space-y-5">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div className="flex gap-6 text-sm">
                <div>
                  <p className="text-xs uppercase tracking-[0.06em] text-ink-faint">{t("invoices.issued")}</p>
                  <p className="text-ink">{formatDate(data.issueDate)}</p>
                </div>
                <div>
                  <p className="text-xs uppercase tracking-[0.06em] text-ink-faint">{t("invoices.due")}</p>
                  <p className="text-ink">{formatDate(data.dueDate)}</p>
                </div>
              </div>
              <span
                className={`rounded-full px-2.5 py-1 text-xs font-medium ${INVOICE_STATUS_STYLES[data.status] ?? "bg-line text-ink-muted"}`}
              >
                {translateInvoiceStatus(data.status, t)}
              </span>
            </div>

            <table className="w-full text-start text-sm">
              <thead>
                <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                  <th className="py-2">{t("common.description")}</th>
                  <th className="py-2 text-end">{t("invoices.qty")}</th>
                  <th className="py-2 text-end">{t("invoices.unitPrice")}</th>
                  <th className="py-2 text-end">{t("common.total")}</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((item, i) => (
                  <tr key={i} className="border-b border-line last:border-0">
                    <td className="py-2 text-ink">{item.description}</td>
                    <td className="py-2 text-end tabular-nums text-ink-muted">{item.quantity}</td>
                    <td className="py-2 text-end tabular-nums text-ink-muted">{formatCurrency(item.unitPrice)}</td>
                    <td className="py-2 text-end tabular-nums text-ink">{formatCurrency(item.total)}</td>
                  </tr>
                ))}
              </tbody>
            </table>

            <div className="ms-auto max-w-xs space-y-1 text-sm">
              <div className="flex justify-between text-ink-muted">
                <span>{t("invoices.subtotal")}</span>
                <span className="tabular-nums">{formatCurrency(data.subTotal)}</span>
              </div>
              <div className="flex justify-between text-ink-muted">
                <span>{t("invoices.discount")}</span>
                <span className="tabular-nums">−{formatCurrency(data.discount)}</span>
              </div>
              <div className="flex justify-between text-ink-muted">
                <span>{t("invoices.tax")}</span>
                <span className="tabular-nums">+{formatCurrency(data.tax)}</span>
              </div>
              <div className="flex justify-between border-t border-line pt-1 font-medium text-ink">
                <span>{t("common.total")}</span>
                <span className="tabular-nums">{formatCurrency(data.total)}</span>
              </div>
              <div className="flex justify-between text-ink-muted">
                <span>{t("invoices.paid")}</span>
                <span className="tabular-nums">{formatCurrency(data.amountPaid)}</span>
              </div>
              <div className="flex justify-between font-medium text-ink">
                <span>{t("invoices.balance")}</span>
                <span className="tabular-nums">{formatCurrency(data.balance)}</span>
              </div>
            </div>

            <div>
              <div className="mb-2 flex items-center justify-between">
                <p className={"text-xs font-medium uppercase tracking-[0.06em] text-ink-faint"}>{t("invoices.payments")}</p>
                {canRecordPayment && (
                  <div className="flex gap-2">
                    <button
                      className={secondaryButtonClass}
                      onClick={handleCreateCheckoutLink}
                      disabled={isCreatingCheckout}
                    >
                      {isCreatingCheckout ? t("invoices.creatingLink") : t("invoices.createPaymentLink")}
                    </button>
                    <button className={primaryButtonClass} onClick={() => setShowPaymentModal(true)}>
                      + {t("invoices.recordPayment")}
                    </button>
                  </div>
                )}
              </div>

              {checkoutError && <p className="mb-2 text-sm text-danger">{checkoutError}</p>}
              {checkoutUrl && (
                <div className="mb-3 flex items-center gap-2 rounded-sm border border-line bg-canvas px-3 py-2 text-xs">
                  <span className="flex-1 truncate text-ink-muted">{checkoutUrl}</span>
                  <button
                    className="font-medium text-harbor-500 hover:underline"
                    onClick={() => navigator.clipboard.writeText(checkoutUrl)}
                  >
                    {t("team.copy")}
                  </button>
                  <a href={checkoutUrl} target="_blank" rel="noreferrer" className="font-medium text-harbor-500 hover:underline">
                    {t("invoices.open")}
                  </a>
                </div>
              )}
              {data.payments.length === 0 ? (
                <p className="text-sm text-ink-faint">{t("invoices.noPaymentsYet")}</p>
              ) : (
                <table className="w-full text-start text-sm">
                  <thead>
                    <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                      <th className="py-2">Date</th>
                      <th className="py-2">Method</th>
                      <th className="py-2">Reference</th>
                      <th className="py-2 text-right">Amount</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.payments.map((p) => (
                      <tr key={p.id} className="border-b border-line last:border-0">
                        <td className="py-2 text-ink-muted">{formatDateTime(p.paidAt)}</td>
                        <td className="py-2 text-ink-muted">{translatePaymentMethod(p.method, t)}</td>
                        <td className="py-2 text-ink-faint">{p.transactionReference || "—"}</td>
                        <td className="py-2 text-end tabular-nums text-ink">{formatCurrency(p.amount)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          </div>
        )}
      </Modal>

      {showPaymentModal && data && (
        <RecordPaymentModal
          invoiceId={data.id}
          invoiceNumber={data.invoiceNumber}
          balance={data.balance}
          onClose={() => setShowPaymentModal(false)}
          onSaved={() => {
            setShowPaymentModal(false);
            refetch();
            onChanged();
          }}
        />
      )}
    </>
  );
}
