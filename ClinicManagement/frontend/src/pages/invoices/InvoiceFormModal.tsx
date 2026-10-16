import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import type { PaginatedList } from "../../lib/types";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";
import { formatCurrency } from "../../lib/format";

interface PatientOption {
  id: string;
  fullName: string;
  patientCode: string;
}

interface LineItem {
  description: string;
  quantity: string;
  unitPrice: string;
}

interface InvoiceFormModalProps {
  onClose: () => void;
  onSaved: () => void;
}

const BLANK_ITEM: LineItem = { description: "", quantity: "1", unitPrice: "" };

export function InvoiceFormModal({ onClose, onSaved }: InvoiceFormModalProps) {
  const { t } = useTranslation();
  const [patientSearch, setPatientSearch] = useState("");
  const [patientOptions, setPatientOptions] = useState<PatientOption[]>([]);
  const [selectedPatient, setSelectedPatient] = useState<PatientOption | null>(null);
  const [dueDate, setDueDate] = useState(new Date().toISOString().slice(0, 10));
  const [discount, setDiscount] = useState("0");
  const [tax, setTax] = useState("0");
  const [items, setItems] = useState<LineItem[]>([{ ...BLANK_ITEM }]);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (selectedPatient || !patientSearch.trim()) {
      setPatientOptions([]);
      return;
    }
    const handle = setTimeout(() => {
      const query = new URLSearchParams({ searchTerm: patientSearch.trim(), pageSize: "8" });
      api
        .get<PaginatedList<PatientOption>>(`/api/patients?${query.toString()}`)
        .then((res) => setPatientOptions(res.items))
        .catch(() => setPatientOptions([]));
    }, 300);
    return () => clearTimeout(handle);
  }, [patientSearch, selectedPatient]);

  function updateItem(index: number, patch: Partial<LineItem>) {
    setItems((prev) => prev.map((item, i) => (i === index ? { ...item, ...patch } : item)));
  }

  function addItem() {
    setItems((prev) => [...prev, { ...BLANK_ITEM }]);
  }

  function removeItem(index: number) {
    setItems((prev) => prev.filter((_, i) => i !== index));
  }

  const subTotal = items.reduce((sum, item) => sum + (Number(item.quantity) || 0) * (Number(item.unitPrice) || 0), 0);
  const total = Math.max(0, subTotal - (Number(discount) || 0) + (Number(tax) || 0));

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!selectedPatient) {
      setError(t("invoices.selectPatientFirst"));
      return;
    }
    setIsSaving(true);
    setError(null);

    try {
      await api.post("/api/invoices", {
        patientId: selectedPatient.id,
        dueDate: new Date(dueDate).toISOString(),
        discount: Number(discount) || 0,
        tax: Number(tax) || 0,
        items: items
          .filter((i) => i.description.trim())
          .map((i) => ({
            description: i.description,
            quantity: Number(i.quantity) || 0,
            unitPrice: Number(i.unitPrice) || 0,
          })),
      });
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("invoices.failedToCreate"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal title={t("invoices.newInvoice")} subtitle={t("invoices.billSubtitle")} onClose={onClose} width="xl">
      <form onSubmit={handleSubmit} className="space-y-5">
        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div className="relative">
            <label className={labelClass}>{t("appointments.patient")}</label>
            {selectedPatient ? (
              <div className="flex items-center justify-between rounded-sm border border-line bg-canvas px-3 py-2 text-sm">
                <span className="text-ink">
                  {selectedPatient.fullName} <span className="text-ink-faint">· {selectedPatient.patientCode}</span>
                </span>
                <button
                  type="button"
                  className="text-xs font-medium text-harbor-500 hover:underline"
                  onClick={() => {
                    setSelectedPatient(null);
                    setPatientSearch("");
                  }}
                >
                  {t("appointments.change")}
                </button>
              </div>
            ) : (
              <>
                <input
                  className={inputClass}
                  placeholder={t("appointments.searchPatientPlaceholder")}
                  value={patientSearch}
                  onChange={(e) => setPatientSearch(e.target.value)}
                  required
                />
                {patientOptions.length > 0 && (
                  <div className="absolute z-10 mt-1 w-full rounded-sm border border-line bg-surface shadow-card">
                    {patientOptions.map((p) => (
                      <button
                        type="button"
                        key={p.id}
                        className="block w-full px-3 py-2 text-start text-sm text-ink hover:bg-canvas"
                        onClick={() => {
                          setSelectedPatient(p);
                          setPatientOptions([]);
                        }}
                      >
                        {p.fullName} <span className="text-ink-faint">· {p.patientCode}</span>
                      </button>
                    ))}
                  </div>
                )}
              </>
            )}
          </div>
          <div>
            <label className={labelClass}>{t("invoices.dueDate")}</label>
            <input
              type="date"
              className={inputClass}
              value={dueDate}
              onChange={(e) => setDueDate(e.target.value)}
              required
            />
          </div>
        </div>

        <div>
          <label className={labelClass}>{t("invoices.lineItems")}</label>
          <div className="space-y-2 rounded-sm border border-line p-3">
            {items.map((item, i) => (
              <div key={i} className="flex flex-wrap items-center gap-2">
                <input
                  className={`${inputClass} flex-1`}
                  placeholder={t("common.description")}
                  value={item.description}
                  onChange={(e) => updateItem(i, { description: e.target.value })}
                />
                <input
                  type="number"
                  min={1}
                  className={`${inputClass} w-20`}
                  placeholder={t("invoices.qty")}
                  value={item.quantity}
                  onChange={(e) => updateItem(i, { quantity: e.target.value })}
                />
                <input
                  type="number"
                  min={0}
                  step="0.01"
                  className={`${inputClass} w-28`}
                  placeholder={t("invoices.unitPrice")}
                  value={item.unitPrice}
                  onChange={(e) => updateItem(i, { unitPrice: e.target.value })}
                />
                <span className="w-20 text-end text-sm tabular-nums text-ink-muted">
                  {formatCurrency((Number(item.quantity) || 0) * (Number(item.unitPrice) || 0))}
                </span>
                <button
                  type="button"
                  className="text-xs font-medium text-danger hover:underline"
                  onClick={() => removeItem(i)}
                  disabled={items.length === 1}
                >
                  {t("invoices.remove")}
                </button>
              </div>
            ))}
            <button type="button" className="text-xs font-medium text-harbor-500 hover:underline" onClick={addItem}>
              + {t("invoices.addLineItem")}
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className={labelClass}>{t("invoices.discount")}</label>
            <input
              type="number"
              min={0}
              step="0.01"
              className={inputClass}
              value={discount}
              onChange={(e) => setDiscount(e.target.value)}
            />
          </div>
          <div>
            <label className={labelClass}>{t("invoices.tax")}</label>
            <input
              type="number"
              min={0}
              step="0.01"
              className={inputClass}
              value={tax}
              onChange={(e) => setTax(e.target.value)}
            />
          </div>
        </div>

        <div className="flex items-center justify-between rounded-sm bg-canvas px-4 py-3 text-sm">
          <span className="text-ink-muted">{t("invoices.subtotal")} {formatCurrency(subTotal)}</span>
          <span className="font-display text-lg text-ink">{t("common.total")} {formatCurrency(total)}</span>
        </div>

        <div className="flex justify-end gap-2 border-t border-line pt-5">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button type="submit" className={primaryButtonClass} disabled={isSaving}>
            {isSaving ? t("invoices.creating") : t("invoices.createInvoice")}
          </button>
        </div>
      </form>
    </Modal>
  );
}
