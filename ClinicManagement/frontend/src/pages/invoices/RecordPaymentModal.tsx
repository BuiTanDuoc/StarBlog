import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";
import { PAYMENT_METHOD_LABELS, translatePaymentMethod, formatCurrency } from "../../lib/format";

interface RecordPaymentModalProps {
  invoiceId: string;
  invoiceNumber: string;
  balance: number;
  onClose: () => void;
  onSaved: () => void;
}

export function RecordPaymentModal({ invoiceId, invoiceNumber, balance, onClose, onSaved }: RecordPaymentModalProps) {
  const { t } = useTranslation();
  const [amount, setAmount] = useState(String(balance));
  const [method, setMethod] = useState("Cash");
  const [transactionReference, setTransactionReference] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setIsSaving(true);
    setError(null);
    try {
      await api.post(`/api/invoices/${invoiceId}/payments`, {
        amount: Number(amount) || 0,
        method,
        transactionReference: transactionReference || null,
      });
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("invoices.failedToRecordPayment"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal title={t("invoices.recordPayment")} subtitle={t("invoices.recordPaymentSubtitle", { number: invoiceNumber, balance: formatCurrency(balance) })} onClose={onClose} width="md">
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

        <div>
          <label className={labelClass}>{t("common.amount")}</label>
          <input
            type="number"
            min={0.01}
            step="0.01"
            max={balance}
            className={inputClass}
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            required
          />
        </div>
        <div>
          <label className={labelClass}>{t("invoices.method")}</label>
          <select className={inputClass} value={method} onChange={(e) => setMethod(e.target.value)}>
            {Object.keys(PAYMENT_METHOD_LABELS).map((value) => (
              <option key={value} value={value}>
                {translatePaymentMethod(value, t)}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className={labelClass}>{t("invoices.transactionReferenceOptional")}</label>
          <input
            className={inputClass}
            value={transactionReference}
            onChange={(e) => setTransactionReference(e.target.value)}
            maxLength={100}
          />
        </div>

        <div className="flex justify-end gap-2 border-t border-line pt-5">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button type="submit" className={primaryButtonClass} disabled={isSaving}>
            {isSaving ? t("invoices.recording") : t("invoices.recordPayment")}
          </button>
        </div>
      </form>
    </Modal>
  );
}
