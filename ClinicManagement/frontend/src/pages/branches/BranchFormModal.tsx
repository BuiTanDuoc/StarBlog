import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";

export interface BranchDto {
  id: string;
  name: string;
  address: string;
  phone: string;
  email?: string | null;
  isActive: boolean;
}

interface BranchFormModalProps {
  branch?: BranchDto;
  onClose: () => void;
  onSaved: () => void;
}

const BLANK_FORM = {
  name: "",
  address: "",
  phone: "",
  email: "",
  isActive: true,
};

export function BranchFormModal({ branch, onClose, onSaved }: BranchFormModalProps) {
  const { t } = useTranslation();
  const isEdit = !!branch;
  const [form, setForm] = useState(BLANK_FORM);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!branch) return;
    setForm({
      name: branch.name,
      address: branch.address,
      phone: branch.phone,
      email: branch.email ?? "",
      isActive: branch.isActive,
    });
  }, [branch]);

  function set<K extends keyof typeof BLANK_FORM>(key: K, value: (typeof BLANK_FORM)[K]) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setIsSaving(true);
    setError(null);

    const payload = {
      name: form.name,
      address: form.address,
      phone: form.phone,
      email: form.email || null,
    };

    try {
      if (isEdit) {
        await api.put(`/api/branches/${branch.id}`, { id: branch.id, ...payload, isActive: form.isActive });
      } else {
        await api.post("/api/branches", payload);
      }
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("branches.failedToSave"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      title={isEdit ? t("branches.editBranch") : t("branches.addBranch")}
      subtitle={isEdit ? t("branches.updateDetails") : t("branches.addNewBranch")}
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
            maxLength={200}
          />
        </div>
        <div>
          <label className={labelClass}>{t("common.address")}</label>
          <input
            className={inputClass}
            value={form.address}
            onChange={(e) => set("address", e.target.value)}
            required
            maxLength={500}
          />
        </div>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className={labelClass}>{t("common.phone")}</label>
            <input
              className={inputClass}
              value={form.phone}
              onChange={(e) => set("phone", e.target.value)}
              required
              maxLength={30}
            />
          </div>
          <div>
            <label className={labelClass}>{t("common.email")}</label>
            <input
              type="email"
              className={inputClass}
              value={form.email}
              onChange={(e) => set("email", e.target.value)}
            />
          </div>
        </div>

        {isEdit && (
          <label className="flex items-center gap-2 text-sm text-ink">
            <input type="checkbox" checked={form.isActive} onChange={(e) => set("isActive", e.target.checked)} />
            {t("common.active")}
          </label>
        )}

        <div className="flex justify-end gap-2 border-t border-line pt-5">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button type="submit" className={primaryButtonClass} disabled={isSaving}>
            {isSaving ? t("common.saving") : isEdit ? t("fields.saveChanges") : t("branches.addBranch")}
          </button>
        </div>
      </form>
    </Modal>
  );
}
