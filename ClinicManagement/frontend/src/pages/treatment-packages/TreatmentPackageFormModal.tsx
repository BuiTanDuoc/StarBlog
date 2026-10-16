import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";

export interface TreatmentPackageDto {
  id: string;
  name: string;
  description?: string | null;
  sessionsIncluded: number;
  price: number;
  validityDays: number;
  isActive: boolean;
}

interface FormModalProps {
  package?: TreatmentPackageDto;
  onClose: () => void;
  onSaved: () => void;
}

export function TreatmentPackageFormModal({ package: pkg, onClose, onSaved }: FormModalProps) {
  const { t } = useTranslation();
  const isEdit = !!pkg;
  const [name, setName] = useState(pkg?.name ?? "");
  const [description, setDescription] = useState(pkg?.description ?? "");
  const [sessionsIncluded, setSessionsIncluded] = useState(pkg?.sessionsIncluded ?? 10);
  const [price, setPrice] = useState(pkg?.price ?? 0);
  const [validityDays, setValidityDays] = useState(pkg?.validityDays ?? 365);
  const [isActive, setIsActive] = useState(pkg?.isActive ?? true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setIsSaving(true);
    try {
      const payload = { name, description: description || null, sessionsIncluded, price, validityDays };
      if (isEdit) {
        await api.put(`/api/treatmentpackages/${pkg.id}`, { id: pkg.id, ...payload, isActive });
      } else {
        await api.post("/api/treatmentpackages", payload);
      }
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("treatmentPackages.failedToSave"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal title={isEdit ? t("treatmentPackages.editPackage") : t("treatmentPackages.newTreatmentPackage")} onClose={onClose} width="md">
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

        <div>
          <label className={labelClass}>{t("common.name")}</label>
          <input
            className={inputClass}
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder={t("treatmentPackages.namePlaceholder")}
            maxLength={150}
            required
          />
        </div>

        <div>
          <label className={labelClass}>{t("expenses.descriptionOptional")}</label>
          <textarea
            className={inputClass}
            rows={2}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            maxLength={1000}
          />
        </div>

        <div className="grid grid-cols-3 gap-3">
          <div>
            <label className={labelClass}>{t("treatmentPackages.sessions")}</label>
            <input
              type="number"
              min={1}
              className={inputClass}
              value={sessionsIncluded}
              onChange={(e) => setSessionsIncluded(Number(e.target.value))}
              required
            />
          </div>
          <div>
            <label className={labelClass}>{t("treatmentPackages.price")}</label>
            <input
              type="number"
              min={0}
              step="0.01"
              className={inputClass}
              value={price}
              onChange={(e) => setPrice(Number(e.target.value))}
              required
            />
          </div>
          <div>
            <label className={labelClass}>{t("treatmentPackages.validForDays")}</label>
            <input
              type="number"
              min={1}
              className={inputClass}
              value={validityDays}
              onChange={(e) => setValidityDays(Number(e.target.value))}
              required
            />
          </div>
        </div>

        {isEdit && (
          <label className="flex items-center gap-2 text-sm text-ink">
            <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />
            {t("treatmentPackages.activeAvailableHint")}
          </label>
        )}

        <div className="flex justify-end gap-2 border-t border-line pt-5">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button type="submit" className={primaryButtonClass} disabled={isSaving}>
            {isSaving ? t("common.saving") : isEdit ? t("fields.saveChanges") : t("treatmentPackages.createPackage")}
          </button>
        </div>
      </form>
    </Modal>
  );
}
