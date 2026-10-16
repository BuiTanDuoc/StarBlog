import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";
import type { TemplateField } from "../../components/DynamicTemplateFields";

export interface MedicalRecordTemplateDto {
  id: string;
  name: string;
  specialization: string;
  isActive: boolean;
  fieldsSchemaJson: string;
}

interface TemplateFormModalProps {
  template?: MedicalRecordTemplateDto;
  onClose: () => void;
  onSaved: () => void;
}

const FIELD_TYPES: TemplateField["type"][] = ["text", "number", "textarea", "date"];

function parseSchema(json: string): TemplateField[] {
  try {
    const parsed = JSON.parse(json);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

/** "Visual Acuity (Left)" -> "visualAcuityLeft" — a sensible default the clinic owner can still edit. */
function toKey(label: string): string {
  const words = label
    .trim()
    .split(/[^a-zA-Z0-9]+/)
    .filter(Boolean);
  if (words.length === 0) return "";
  return words
    .map((w, i) => (i === 0 ? w.charAt(0).toLowerCase() + w.slice(1) : w.charAt(0).toUpperCase() + w.slice(1)))
    .join("");
}

export function MedicalRecordTemplateFormModal({ template, onClose, onSaved }: TemplateFormModalProps) {
  const { t } = useTranslation();
  const isEdit = !!template;
  const [name, setName] = useState(template?.name ?? "");
  const [specialization, setSpecialization] = useState(template?.specialization ?? "");
  const [isActive, setIsActive] = useState(template?.isActive ?? true);
  const [fields, setFields] = useState<TemplateField[]>(
    template ? parseSchema(template.fieldsSchemaJson) : [],
  );
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function addField() {
    setFields((prev) => [...prev, { key: "", label: "", type: "text" }]);
  }

  function updateField(index: number, patch: Partial<TemplateField>) {
    setFields((prev) =>
      prev.map((f, i) => {
        if (i !== index) return f;
        const next = { ...f, ...patch };
        // Auto-fill the key from the label unless the clinic owner has already customized it.
        if (patch.label !== undefined && (f.key === "" || f.key === toKey(f.label))) {
          next.key = toKey(patch.label);
        }
        return next;
      }),
    );
  }

  function removeField(index: number) {
    setFields((prev) => prev.filter((_, i) => i !== index));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);

    const cleanFields = fields.filter((f) => f.key.trim() && f.label.trim());
    if (cleanFields.length === 0) {
      setError(t("templates.addAtLeastOneField"));
      return;
    }

    setIsSaving(true);
    try {
      const payload = {
        name,
        specialization,
        fieldsSchemaJson: JSON.stringify(cleanFields),
      };
      if (isEdit) {
        await api.put(`/api/medicalrecordtemplates/${template.id}`, { id: template.id, ...payload, isActive });
      } else {
        await api.post("/api/medicalrecordtemplates", payload);
      }
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("templates.failedToSave"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      title={isEdit ? t("templates.editTemplate") : t("templates.newTemplate")}
      subtitle={t("templates.formSubtitle")}
      onClose={onClose}
      width="xl"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className={labelClass}>{t("templates.templateName")}</label>
            <input
              className={inputClass}
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder={t("templates.templateNamePlaceholder")}
              maxLength={150}
              required
            />
          </div>
          <div>
            <label className={labelClass}>{t("doctors.specialization")}</label>
            <input
              className={inputClass}
              value={specialization}
              onChange={(e) => setSpecialization(e.target.value)}
              placeholder={t("templates.specializationPlaceholder")}
              maxLength={150}
              required
            />
            <p className="mt-1 text-xs text-ink-faint">
              {t("templates.specializationHint")}
            </p>
          </div>
        </div>

        {isEdit && (
          <label className="flex items-center gap-2 text-sm text-ink">
            <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />
            {t("templates.activeVisibleHint")}
          </label>
        )}

        <div className="space-y-3 rounded-sm border border-line p-3">
          <p className={labelClass}>{t("templates.fields")}</p>
          {fields.map((field, i) => (
            <div key={i} className="flex items-end gap-2">
              <div className="flex-1">
                <label className={labelClass}>{t("templates.label")}</label>
                <input
                  className={inputClass}
                  value={field.label}
                  onChange={(e) => updateField(i, { label: e.target.value })}
                  placeholder={t("templates.labelPlaceholder")}
                  maxLength={150}
                />
              </div>
              <div className="w-32">
                <label className={labelClass}>{t("templates.key")}</label>
                <input
                  className={inputClass}
                  value={field.key}
                  onChange={(e) => updateField(i, { key: e.target.value })}
                  placeholder="visualAcuityLeft"
                />
              </div>
              <div className="w-32">
                <label className={labelClass}>{t("templates.type")}</label>
                <select
                  className={inputClass}
                  value={field.type}
                  onChange={(e) => updateField(i, { type: e.target.value as TemplateField["type"] })}
                >
                  {FIELD_TYPES.map((ft) => (
                    <option key={ft} value={ft}>
                      {t(`templates.fieldTypes.${ft}`, ft)}
                    </option>
                  ))}
                </select>
              </div>
              <button
                type="button"
                className="mb-2 text-xs font-medium text-danger hover:underline"
                onClick={() => removeField(i)}
              >
                {t("invoices.remove")}
              </button>
            </div>
          ))}
          <button type="button" className="text-xs font-medium text-harbor-500 hover:underline" onClick={addField}>
            + {t("templates.addField")}
          </button>
        </div>

        <div className="flex justify-end gap-2 border-t border-line pt-5">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button type="submit" className={primaryButtonClass} disabled={isSaving}>
            {isSaving ? t("common.saving") : isEdit ? t("fields.saveChanges") : t("templates.createTemplate")}
          </button>
        </div>
      </form>
    </Modal>
  );
}
