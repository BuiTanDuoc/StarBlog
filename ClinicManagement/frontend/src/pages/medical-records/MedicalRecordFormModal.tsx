import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import type { PaginatedList } from "../../lib/types";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";
import { toDateInputValue } from "../../lib/format";
import { DynamicTemplateFields, parseTemplateSchema, type TemplateField } from "../../components/DynamicTemplateFields";

export interface MedicalRecordDto {
  id: string;
  patientId: string;
  doctorId: string;
  doctorName: string;
  diagnosis: string;
  symptoms?: string | null;
  treatmentPlan?: string | null;
  notes?: string | null;
  followUpDate?: string | null;
  templateId?: string | null;
  templateName?: string | null;
  customFieldsJson?: string | null;
  createdAt: string;
}

interface TemplateOption {
  id: string;
  name: string;
  specialization: string;
  fieldsSchemaJson: string;
}

interface DoctorOption {
  id: string;
  fullName: string;
}

interface MedicalRecordFormModalProps {
  patientId: string;
  record?: MedicalRecordDto;
  onClose: () => void;
  onSaved: () => void;
}

const BLANK_FORM = {
  doctorId: "",
  diagnosis: "",
  symptoms: "",
  treatmentPlan: "",
  notes: "",
  followUpDate: "",
};

export function MedicalRecordFormModal({ patientId, record, onClose, onSaved }: MedicalRecordFormModalProps) {
  const { t } = useTranslation();
  const isEdit = !!record;
  const [form, setForm] = useState(BLANK_FORM);
  const [doctors, setDoctors] = useState<DoctorOption[]>([]);
  const [templates, setTemplates] = useState<TemplateOption[]>([]);
  const [templateId, setTemplateId] = useState("");
  const [customFields, setCustomFields] = useState<Record<string, string>>({});
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .get<PaginatedList<DoctorOption>>("/api/doctors?pageSize=100&status=Active")
      .then((res) => setDoctors(res.items))
      .catch(() => setDoctors([]));
    api
      .get<TemplateOption[]>("/api/medicalrecordtemplates?activeOnly=true")
      .then(setTemplates)
      .catch(() => setTemplates([]));
  }, []);

  useEffect(() => {
    if (!record) return;
    setForm({
      doctorId: record.doctorId,
      diagnosis: record.diagnosis,
      symptoms: record.symptoms ?? "",
      treatmentPlan: record.treatmentPlan ?? "",
      notes: record.notes ?? "",
      followUpDate: record.followUpDate ? toDateInputValue(record.followUpDate) : "",
    });
    setTemplateId(record.templateId ?? "");
    if (record.customFieldsJson) {
      try {
        setCustomFields(JSON.parse(record.customFieldsJson));
      } catch {
        setCustomFields({});
      }
    }
  }, [record]);

  const selectedTemplate = templates.find((tpl) => tpl.id === templateId) ?? null;
  const templateSchema: TemplateField[] = selectedTemplate ? parseTemplateSchema(selectedTemplate.fieldsSchemaJson) : [];

  function set<K extends keyof typeof BLANK_FORM>(key: K, value: string) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setIsSaving(true);
    setError(null);

    const shared = {
      diagnosis: form.diagnosis,
      symptoms: form.symptoms || null,
      treatmentPlan: form.treatmentPlan || null,
      notes: form.notes || null,
      followUpDate: form.followUpDate ? new Date(form.followUpDate).toISOString() : null,
      templateId: templateId || null,
      customFieldsJson: templateId && Object.keys(customFields).length > 0 ? JSON.stringify(customFields) : null,
    };

    try {
      if (isEdit) {
        await api.put(`/api/medicalrecords/${record.id}`, { id: record.id, ...shared });
      } else {
        await api.post("/api/medicalrecords", { patientId, doctorId: form.doctorId, ...shared });
      }
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("medicalRecords.failedToSaveRecord"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      title={isEdit ? t("medicalRecords.editRecord") : t("medicalRecords.addRecord")}
      subtitle={isEdit ? t("medicalRecords.updateVisitNotes") : t("medicalRecords.logNewDiagnosis")}
      onClose={onClose}
      width="xl"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

        {!isEdit && (
          <div>
            <label className={labelClass}>{t("appointments.doctor")}</label>
            <select
              className={inputClass}
              value={form.doctorId}
              onChange={(e) => set("doctorId", e.target.value)}
              required
            >
              <option value="" disabled>
                {t("appointments.selectDoctor")}
              </option>
              {doctors.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.fullName}
                </option>
              ))}
            </select>
          </div>
        )}

        <div>
          <label className={labelClass}>{t("medicalRecords.diagnosis")}</label>
          <textarea
            className={`${inputClass} min-h-[70px]`}
            value={form.diagnosis}
            onChange={(e) => set("diagnosis", e.target.value)}
            required
            maxLength={1000}
          />
        </div>
        <div>
          <label className={labelClass}>{t("medicalRecords.symptoms")}</label>
          <textarea
            className={`${inputClass} min-h-[60px]`}
            value={form.symptoms}
            onChange={(e) => set("symptoms", e.target.value)}
          />
        </div>
        <div>
          <label className={labelClass}>{t("medicalRecords.treatmentPlan")}</label>
          <textarea
            className={`${inputClass} min-h-[60px]`}
            value={form.treatmentPlan}
            onChange={(e) => set("treatmentPlan", e.target.value)}
          />
        </div>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className={labelClass}>{t("common.notes")}</label>
            <input className={inputClass} value={form.notes} onChange={(e) => set("notes", e.target.value)} />
          </div>
          <div>
            <label className={labelClass}>{t("medicalRecords.followUpDateOptional")}</label>
            <input
              type="date"
              className={inputClass}
              value={form.followUpDate}
              onChange={(e) => set("followUpDate", e.target.value)}
            />
          </div>
        </div>

        <div>
          <label className={labelClass}>{t("medicalRecords.examTemplateOptional")}</label>
          <select
            className={inputClass}
            value={templateId}
            onChange={(e) => {
              setTemplateId(e.target.value);
              setCustomFields({});
            }}
          >
            <option value="">{t("medicalRecords.noTemplate")}</option>
            {templates.map((tpl) => (
              <option key={tpl.id} value={tpl.id}>
                {tpl.name} ({tpl.specialization})
              </option>
            ))}
          </select>
        </div>

        {templateSchema.length > 0 && (
          <DynamicTemplateFields schema={templateSchema} values={customFields} onChange={setCustomFields} />
        )}

        <div className="flex justify-end gap-2 border-t border-line pt-5">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button type="submit" className={primaryButtonClass} disabled={isSaving}>
            {isSaving ? t("common.saving") : isEdit ? t("fields.saveChanges") : t("medicalRecords.addRecord")}
          </button>
        </div>
      </form>
    </Modal>
  );
}
