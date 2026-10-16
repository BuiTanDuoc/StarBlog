import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";
import { PAYMENT_METHOD_LABELS, translatePaymentMethod, toDateInputValue } from "../../lib/format";

export interface ExpenseDto {
  id: string;
  branchId?: string | null;
  category: string;
  description?: string | null;
  amount: number;
  expenseDate: string;
  method: string;
}

interface Branch {
  id: string;
  name: string;
}

interface ExpenseFormModalProps {
  expense?: ExpenseDto;
  branches: Branch[];
  onClose: () => void;
  onSaved: () => void;
}

const COMMON_CATEGORIES = ["Rent", "Utilities", "Supplies", "Salaries", "Maintenance", "Marketing", "Other"];

const BLANK_FORM = {
  branchId: "",
  category: "",
  description: "",
  amount: "",
  expenseDate: new Date().toISOString().slice(0, 10),
  method: "Cash",
};

export function ExpenseFormModal({ expense, branches, onClose, onSaved }: ExpenseFormModalProps) {
  const { t } = useTranslation();
  const isEdit = !!expense;
  const [form, setForm] = useState(BLANK_FORM);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!expense) return;
    setForm({
      branchId: expense.branchId ?? "",
      category: expense.category,
      description: expense.description ?? "",
      amount: String(expense.amount),
      expenseDate: toDateInputValue(expense.expenseDate),
      method: expense.method,
    });
  }, [expense]);

  function set<K extends keyof typeof BLANK_FORM>(key: K, value: string) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setIsSaving(true);
    setError(null);

    const payload = {
      category: form.category,
      description: form.description || null,
      amount: Number(form.amount) || 0,
      expenseDate: new Date(form.expenseDate).toISOString(),
      method: form.method,
    };

    try {
      if (isEdit) {
        await api.put(`/api/expenses/${expense.id}`, { id: expense.id, ...payload });
      } else {
        await api.post("/api/expenses", { ...payload, branchId: form.branchId || null });
      }
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("expenses.failedToSave"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      title={isEdit ? t("expenses.editExpense") : t("expenses.addExpense")}
      subtitle={isEdit ? t("expenses.updateRecord") : t("expenses.logExpense")}
      onClose={onClose}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className={labelClass}>{t("expenses.category")}</label>
            <input
              className={inputClass}
              list="expense-categories"
              value={form.category}
              onChange={(e) => set("category", e.target.value)}
              required
              maxLength={100}
            />
            <datalist id="expense-categories">
              {COMMON_CATEGORIES.map((cat) => (
                <option key={cat} value={t(`expenses.categories.${cat}`, cat)} />
              ))}
            </datalist>
          </div>
          <div>
            <label className={labelClass}>{t("common.amount")}</label>
            <input
              type="number"
              min={0.01}
              step="0.01"
              className={inputClass}
              value={form.amount}
              onChange={(e) => set("amount", e.target.value)}
              required
            />
          </div>
          <div>
            <label className={labelClass}>{t("common.date")}</label>
            <input
              type="date"
              className={inputClass}
              value={form.expenseDate}
              onChange={(e) => set("expenseDate", e.target.value)}
              required
            />
          </div>
          <div>
            <label className={labelClass}>{t("expenses.paymentMethod")}</label>
            <select className={inputClass} value={form.method} onChange={(e) => set("method", e.target.value)}>
              {Object.keys(PAYMENT_METHOD_LABELS).map((value) => (
                <option key={value} value={value}>
                  {translatePaymentMethod(value, t)}
                </option>
              ))}
            </select>
          </div>
          {!isEdit && (
            <div>
              <label className={labelClass}>{t("expenses.branchOptional")}</label>
              <select className={inputClass} value={form.branchId} onChange={(e) => set("branchId", e.target.value)}>
                <option value="">{t("expenses.clinicWide")}</option>
                {branches.map((b) => (
                  <option key={b.id} value={b.id}>
                    {b.name}
                  </option>
                ))}
              </select>
            </div>
          )}
        </div>
        <div>
          <label className={labelClass}>{t("expenses.descriptionOptional")}</label>
          <input
            className={inputClass}
            value={form.description}
            onChange={(e) => set("description", e.target.value)}
            maxLength={500}
          />
        </div>

        <div className="flex justify-end gap-2 border-t border-line pt-5">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button type="submit" className={primaryButtonClass} disabled={isSaving}>
            {isSaving ? t("common.saving") : isEdit ? t("fields.saveChanges") : t("expenses.addExpense")}
          </button>
        </div>
      </form>
    </Modal>
  );
}
