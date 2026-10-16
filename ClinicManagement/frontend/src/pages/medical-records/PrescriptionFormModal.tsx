import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";

interface PrescriptionItem {
  medicineName: string;
  dosage: string;
  frequency: string;
  duration: string;
  instructions: string;
}

interface PrescriptionFormModalProps {
  patientId: string;
  doctorId: string;
  medicalRecordId: string;
  onClose: () => void;
  onSaved: () => void;
}

const BLANK_ITEM: PrescriptionItem = { medicineName: "", dosage: "", frequency: "", duration: "", instructions: "" };

export function PrescriptionFormModal({
  patientId,
  doctorId,
  medicalRecordId,
  onClose,
  onSaved,
}: PrescriptionFormModalProps) {
  const { t } = useTranslation();
  const [items, setItems] = useState<PrescriptionItem[]>([{ ...BLANK_ITEM }]);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  // Populated from the response's `warnings` field (see IDrugSafetyChecker on the backend) —
  // informational only, the prescription is already saved by the time these show up.
  const [warnings, setWarnings] = useState<string[] | null>(null);

  function updateItem(index: number, patch: Partial<PrescriptionItem>) {
    setItems((prev) => prev.map((item, i) => (i === index ? { ...item, ...patch } : item)));
  }

  function addItem() {
    setItems((prev) => [...prev, { ...BLANK_ITEM }]);
  }

  function removeItem(index: number) {
    setItems((prev) => prev.filter((_, i) => i !== index));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setIsSaving(true);
    setError(null);

    try {
      const response = await api.post<{ warnings?: string[] }>("/api/prescriptions", {
        medicalRecordId,
        patientId,
        doctorId,
        items: items
          .filter((i) => i.medicineName.trim())
          .map((i) => ({
            medicineName: i.medicineName,
            dosage: i.dosage,
            frequency: i.frequency,
            duration: i.duration,
            instructions: i.instructions || null,
          })),
      });

      if (response.warnings && response.warnings.length > 0) {
        // Don't close yet — the doctor should see these before moving on. onSaved() (which
        // refreshes the chart behind the modal) still runs now since the prescription is
        // already committed either way.
        setWarnings(response.warnings);
        onSaved();
      } else {
        onSaved();
        onClose();
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("medicalRecords.failedToSavePrescription"));
    } finally {
      setIsSaving(false);
    }
  }

  if (warnings) {
    return (
      <Modal title={t("medicalRecords.prescriptionSavedReview")} onClose={onClose} width="lg">
        <div className="space-y-3">
          <p className="text-sm text-ink-muted">
            {t("medicalRecords.prescriptionSavedExplainer")}
          </p>
          <ul className="space-y-2">
            {warnings.map((w, i) => (
              <li key={i} className="rounded-sm bg-warning-light px-3.5 py-2.5 text-sm text-warning">
                {w}
              </li>
            ))}
          </ul>
          <div className="flex justify-end border-t border-line pt-5">
            <button className={primaryButtonClass} onClick={onClose}>
              {t("medicalRecords.acknowledge")}
            </button>
          </div>
        </div>
      </Modal>
    );
  }

  return (
    <Modal title={t("medicalRecords.newPrescription")} subtitle={t("medicalRecords.addMedicinesSubtitle")} onClose={onClose} width="xl">
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

        <div className="space-y-3 rounded-sm border border-line p-3">
          {items.map((item, i) => (
            <div key={i} className="space-y-2 border-b border-line pb-3 last:border-0 last:pb-0">
              <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
                <div>
                  <label className={labelClass}>{t("medicalRecords.medicine")}</label>
                  <input
                    className={inputClass}
                    value={item.medicineName}
                    onChange={(e) => updateItem(i, { medicineName: e.target.value })}
                    maxLength={200}
                  />
                </div>
                <div>
                  <label className={labelClass}>{t("medicalRecords.dosage")}</label>
                  <input
                    className={inputClass}
                    placeholder={t("medicalRecords.dosagePlaceholder")}
                    value={item.dosage}
                    onChange={(e) => updateItem(i, { dosage: e.target.value })}
                    maxLength={100}
                  />
                </div>
                <div>
                  <label className={labelClass}>{t("medicalRecords.frequency")}</label>
                  <input
                    className={inputClass}
                    placeholder={t("medicalRecords.frequencyPlaceholder")}
                    value={item.frequency}
                    onChange={(e) => updateItem(i, { frequency: e.target.value })}
                    maxLength={100}
                  />
                </div>
                <div>
                  <label className={labelClass}>{t("medicalRecords.duration")}</label>
                  <input
                    className={inputClass}
                    placeholder={t("medicalRecords.durationPlaceholder")}
                    value={item.duration}
                    onChange={(e) => updateItem(i, { duration: e.target.value })}
                    maxLength={100}
                  />
                </div>
              </div>
              <div>
                <label className={labelClass}>{t("medicalRecords.instructionsOptional")}</label>
                <input
                  className={inputClass}
                  value={item.instructions}
                  onChange={(e) => updateItem(i, { instructions: e.target.value })}
                />
              </div>
              <button
                type="button"
                className="text-xs font-medium text-danger hover:underline"
                onClick={() => removeItem(i)}
                disabled={items.length === 1}
              >
                {t("medicalRecords.removeMedicine")}
              </button>
            </div>
          ))}
          <button type="button" className="text-xs font-medium text-harbor-500 hover:underline" onClick={addItem}>
            + {t("medicalRecords.addMedicine")}
          </button>
        </div>

        <div className="flex justify-end gap-2 border-t border-line pt-5">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button type="submit" className={primaryButtonClass} disabled={isSaving}>
            {isSaving ? t("common.saving") : t("medicalRecords.savePrescription")}
          </button>
        </div>
      </form>
    </Modal>
  );
}
