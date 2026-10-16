import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";

export interface DepartmentDto {
  id: string;
  name: string;
  branchId: string;
}

interface Branch {
  id: string;
  name: string;
}

interface DepartmentFormModalProps {
  department?: DepartmentDto;
  branches: Branch[];
  onClose: () => void;
  onSaved: () => void;
}

const BLANK_FORM = { name: "", branchId: "" };

export function DepartmentFormModal({ department, branches, onClose, onSaved }: DepartmentFormModalProps) {
  const { t } = useTranslation();
  const isEdit = !!department;
  const [form, setForm] = useState(BLANK_FORM);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!department) return;
    setForm({ name: department.name, branchId: department.branchId });
  }, [department]);

  function set<K extends keyof typeof BLANK_FORM>(key: K, value: string) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setIsSaving(true);
    setError(null);

    try {
      if (isEdit) {
        await api.put(`/api/departments/${department.id}`, {
          id: department.id,
          name: form.name,
          branchId: form.branchId,
        });
      } else {
        await api.post("/api/departments", { name: form.name, branchId: form.branchId });
      }
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("departments.failedToSave"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      title={isEdit ? t("departments.editDepartment") : t("departments.addDepartment")}
      subtitle={isEdit ? t("departments.updateDepartment") : t("departments.addToBranch")}
      onClose={onClose}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

        <div>
          <label className={labelClass}>{t("common.name")}</label>
          <input
            className={inputClass}
            value={form.name}
            onChange={(e) => set("name", e.target.value)}
            required
            maxLength={150}
          />
        </div>
        <div>
          <label className={labelClass}>{t("appointments.branch")}</label>
          <select
            className={inputClass}
            value={form.branchId}
            onChange={(e) => set("branchId", e.target.value)}
            required
          >
            <option value="" disabled>
              {t("appointments.selectBranch")}
            </option>
            {branches.map((b) => (
              <option key={b.id} value={b.id}>
                {b.name}
              </option>
            ))}
          </select>
        </div>

        <div className="flex justify-end gap-2 border-t border-line pt-5">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button type="submit" className={primaryButtonClass} disabled={isSaving}>
            {isSaving ? t("common.saving") : isEdit ? t("fields.saveChanges") : t("departments.addDepartment")}
          </button>
        </div>
      </form>
    </Modal>
  );
}
