import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";
import { toDateInputValue } from "../../lib/format";

export interface PatientDetail {
  id: string;
  patientCode: string;
  nationalId?: string | null;
  fullName: string;
  gender: "Male" | "Female" | "Other";
  dateOfBirth: string;
  phone: string;
  address?: string | null;
  bloodGroup: string;
  emergencyContactName?: string | null;
  emergencyContactPhone?: string | null;
  allergies?: string | null;
  medicalHistory?: string | null;
  insuranceProvider?: string | null;
  insuranceNumber?: string | null;
}

interface PatientFormModalProps {
  /** Present for edit, absent for create. */
  patientId?: string;
  onClose: () => void;
  onSaved: () => void;
}

const BLANK_FORM = {
  nationalId: "",
  fullName: "",
  gender: "Male",
  dateOfBirth: "",
  phone: "",
  address: "",
  bloodGroup: "Unknown",
  emergencyContactName: "",
  emergencyContactPhone: "",
  allergies: "",
  medicalHistory: "",
  insuranceProvider: "",
  insuranceNumber: "",
};

export function PatientFormModal({ patientId, onClose, onSaved }: PatientFormModalProps) {
  const { t } = useTranslation();
  const isEdit = !!patientId;
  const [form, setForm] = useState(BLANK_FORM);
  const [isLoading, setIsLoading] = useState(isEdit);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!patientId) return;
    setIsLoading(true);
    api
      .get<PatientDetail>(`/api/patients/${patientId}`)
      .then((p) =>
        setForm({
          nationalId: p.nationalId ?? "",
          fullName: p.fullName,
          gender: p.gender,
          dateOfBirth: toDateInputValue(p.dateOfBirth),
          phone: p.phone,
          address: p.address ?? "",
          bloodGroup: p.bloodGroup,
          emergencyContactName: p.emergencyContactName ?? "",
          emergencyContactPhone: p.emergencyContactPhone ?? "",
          allergies: p.allergies ?? "",
          medicalHistory: p.medicalHistory ?? "",
          insuranceProvider: p.insuranceProvider ?? "",
          insuranceNumber: p.insuranceNumber ?? "",
        }),
      )
      .catch((err) => setError(err instanceof ApiError ? err.message : t("patients.failedToLoad")))
      .finally(() => setIsLoading(false));
  }, [patientId]);

  function set<K extends keyof typeof BLANK_FORM>(key: K, value: string) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setIsSaving(true);
    setError(null);

    const payload = {
      nationalId: form.nationalId || null,
      fullName: form.fullName,
      gender: form.gender,
      dateOfBirth: form.dateOfBirth,
      phone: form.phone,
      address: form.address || null,
      bloodGroup: form.bloodGroup,
      emergencyContactName: form.emergencyContactName || null,
      emergencyContactPhone: form.emergencyContactPhone || null,
      allergies: form.allergies || null,
      medicalHistory: form.medicalHistory || null,
      insuranceProvider: form.insuranceProvider || null,
      insuranceNumber: form.insuranceNumber || null,
    };

    try {
      if (isEdit) {
        await api.put(`/api/patients/${patientId}`, { id: patientId, ...payload });
      } else {
        await api.post("/api/patients", payload);
      }
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("patients.failedToSave"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      title={isEdit ? t("patients.editPatient") : t("patients.addPatient")}
      subtitle={isEdit ? t("patients.updateRecord") : t("patients.registerNew")}
      onClose={onClose}
      width="xl"
    >
      {isLoading ? (
        <p className="py-8 text-center font-mono text-sm text-ink-muted">{t("common.loading")}</p>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-5">
          {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label className={labelClass}>{t("fields.fullName")}</label>
              <input
                className={inputClass}
                value={form.fullName}
                onChange={(e) => set("fullName", e.target.value)}
                required
                maxLength={200}
              />
            </div>
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
              <label className={labelClass}>{t("patients.gender")}</label>
              <select className={inputClass} value={form.gender} onChange={(e) => set("gender", e.target.value)}>
                <option value="Male">{t("enums.gender.Male")}</option>
                <option value="Female">{t("enums.gender.Female")}</option>
                <option value="Other">{t("enums.gender.Other")}</option>
              </select>
            </div>
            <div>
              <label className={labelClass}>{t("fields.dateOfBirth")}</label>
              <input
                type="date"
                className={inputClass}
                value={form.dateOfBirth}
                onChange={(e) => set("dateOfBirth", e.target.value)}
                required
                max={toDateInputValue(new Date().toISOString())}
              />
            </div>
            <div>
              <label className={labelClass}>{t("fields.nationalId")}</label>
              <input
                className={inputClass}
                value={form.nationalId}
                onChange={(e) => set("nationalId", e.target.value)}
                maxLength={50}
              />
            </div>
            <div>
              <label className={labelClass}>{t("fields.bloodGroup")}</label>
              <select
                className={inputClass}
                value={form.bloodGroup}
                onChange={(e) => set("bloodGroup", e.target.value)}
              >
                <option value="Unknown">{t("enums.bloodGroup.Unknown")}</option>
                <option value="APositive">A+</option>
                <option value="ANegative">A−</option>
                <option value="BPositive">B+</option>
                <option value="BNegative">B−</option>
                <option value="ABPositive">AB+</option>
                <option value="ABNegative">AB−</option>
                <option value="OPositive">O+</option>
                <option value="ONegative">O−</option>
              </select>
            </div>
            <div className="sm:col-span-2">
              <label className={labelClass}>{t("common.address")}</label>
              <input className={inputClass} value={form.address} onChange={(e) => set("address", e.target.value)} />
            </div>
            <div>
              <label className={labelClass}>{t("fields.emergencyContactName")}</label>
              <input
                className={inputClass}
                value={form.emergencyContactName}
                onChange={(e) => set("emergencyContactName", e.target.value)}
              />
            </div>
            <div>
              <label className={labelClass}>{t("fields.emergencyContactPhone")}</label>
              <input
                className={inputClass}
                value={form.emergencyContactPhone}
                onChange={(e) => set("emergencyContactPhone", e.target.value)}
                maxLength={30}
              />
            </div>
            <div>
              <label className={labelClass}>{t("fields.insuranceProvider")}</label>
              <input
                className={inputClass}
                value={form.insuranceProvider}
                onChange={(e) => set("insuranceProvider", e.target.value)}
              />
            </div>
            <div>
              <label className={labelClass}>{t("fields.insuranceNumber")}</label>
              <input
                className={inputClass}
                value={form.insuranceNumber}
                onChange={(e) => set("insuranceNumber", e.target.value)}
              />
            </div>
            <div className="sm:col-span-2">
              <label className={labelClass}>{t("fields.allergies")}</label>
              <textarea
                className={inputClass}
                rows={2}
                value={form.allergies}
                onChange={(e) => set("allergies", e.target.value)}
              />
            </div>
            <div className="sm:col-span-2">
              <label className={labelClass}>{t("fields.medicalHistory")}</label>
              <textarea
                className={inputClass}
                rows={3}
                value={form.medicalHistory}
                onChange={(e) => set("medicalHistory", e.target.value)}
              />
            </div>
          </div>

          <div className="flex justify-end gap-2 border-t border-line pt-5">
            <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
              {t("common.cancel")}
            </button>
            <button type="submit" className={primaryButtonClass} disabled={isSaving}>
              {isSaving ? t("common.saving") : isEdit ? t("fields.saveChanges") : t("patients.addPatient")}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
