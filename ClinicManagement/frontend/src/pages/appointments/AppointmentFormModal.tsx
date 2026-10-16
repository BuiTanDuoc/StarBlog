import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { DoctorSlotPicker } from "./DoctorSlotPicker";
import { api, ApiError } from "../../lib/api";
import type { PaginatedList } from "../../lib/types";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";

interface PatientOption {
  id: string;
  fullName: string;
  patientCode: string;
  phone: string;
}

interface DoctorOption {
  id: string;
  fullName: string;
  specialization: string;
}

interface Branch {
  id: string;
  name: string;
}

interface Slot {
  start: string;
  end: string;
}

interface AppointmentFormModalProps {
  onClose: () => void;
  onSaved: () => void;
}

export function AppointmentFormModal({ onClose, onSaved }: AppointmentFormModalProps) {
  const { t } = useTranslation();
  const [patientSearch, setPatientSearch] = useState("");
  const [patientOptions, setPatientOptions] = useState<PatientOption[]>([]);
  const [selectedPatient, setSelectedPatient] = useState<PatientOption | null>(null);

  const [doctors, setDoctors] = useState<DoctorOption[]>([]);
  const [doctorId, setDoctorId] = useState("");

  const [branches, setBranches] = useState<Branch[]>([]);
  const [branchId, setBranchId] = useState("");

  const [date, setDate] = useState("");
  const [slot, setSlot] = useState<Slot | null>(null);
  const [notes, setNotes] = useState("");

  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .get<PaginatedList<DoctorOption>>("/api/doctors?pageSize=100&status=Active")
      .then((res) => setDoctors(res.items))
      .catch(() => setDoctors([]));
    api.get<Branch[]>("/api/branches?activeOnly=true").then(setBranches).catch(() => setBranches([]));
  }, []);

  useEffect(() => {
    if (selectedPatient || patientSearch.trim().length < 2) {
      setPatientOptions([]);
      return;
    }
    const handle = setTimeout(() => {
      api
        .get<PaginatedList<PatientOption>>(
          `/api/patients?searchTerm=${encodeURIComponent(patientSearch.trim())}&pageSize=8`,
        )
        .then((res) => setPatientOptions(res.items))
        .catch(() => setPatientOptions([]));
    }, 300);
    return () => clearTimeout(handle);
  }, [patientSearch, selectedPatient]);

  useEffect(() => {
    setSlot(null);
  }, [doctorId, date]);

  const todayIso = new Date().toISOString().slice(0, 10);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!selectedPatient || !doctorId || !branchId || !slot) {
      setError(t("appointments.chooseAllFields"));
      return;
    }
    setIsSaving(true);
    setError(null);
    try {
      await api.post("/api/appointments", {
        patientId: selectedPatient.id,
        doctorId,
        branchId,
        scheduledStart: slot.start,
        scheduledEnd: slot.end,
        notes: notes || null,
      });
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("appointments.failedToBook"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal title={t("appointments.newAppointment")} subtitle={t("appointments.bookSlotSubtitle")} onClose={onClose} width="xl">
      <form onSubmit={handleSubmit} className="space-y-5">
        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

        <div>
          <label className={labelClass}>{t("appointments.patient")}</label>
          {selectedPatient ? (
            <div className="flex items-center justify-between rounded-sm border border-line bg-canvas px-3 py-2">
              <div>
                <p className="text-sm font-medium text-ink">{selectedPatient.fullName}</p>
                <p className="font-mono text-xs text-ink-faint">
                  {selectedPatient.patientCode} · {selectedPatient.phone}
                </p>
              </div>
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
            <div>
              <input
                className={inputClass}
                placeholder={t("appointments.searchPatientPlaceholder")}
                value={patientSearch}
                onChange={(e) => setPatientSearch(e.target.value)}
              />
              {patientOptions.length > 0 && (
                <div className="mt-1 max-h-40 overflow-y-auto rounded-sm border border-line bg-surface shadow-card">
                  {patientOptions.map((p) => (
                    <button
                      key={p.id}
                      type="button"
                      className="block w-full px-3 py-2 text-start text-sm hover:bg-canvas"
                      onClick={() => {
                        setSelectedPatient(p);
                        setPatientOptions([]);
                      }}
                    >
                      <span className="font-medium text-ink">{p.fullName}</span>{" "}
                      <span className="font-mono text-xs text-ink-faint">{p.patientCode}</span>
                    </button>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className={labelClass}>{t("appointments.doctor")}</label>
            <select className={inputClass} value={doctorId} onChange={(e) => setDoctorId(e.target.value)} required>
              <option value="" disabled>
                {t("appointments.selectDoctor")}
              </option>
              {doctors.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.fullName} — {d.specialization}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className={labelClass}>{t("appointments.branch")}</label>
            <select className={inputClass} value={branchId} onChange={(e) => setBranchId(e.target.value)} required>
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
          <div>
            <label className={labelClass}>{t("common.date")}</label>
            <input
              type="date"
              className={inputClass}
              value={date}
              min={todayIso}
              onChange={(e) => setDate(e.target.value)}
              required
            />
          </div>
        </div>

        <DoctorSlotPicker doctorId={doctorId} date={date} selected={slot} onSelect={setSlot} />

        <div>
          <label className={labelClass}>{t("appointments.notesOptional")}</label>
          <textarea className={inputClass} rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
        </div>

        <div className="flex justify-end gap-2 border-t border-line pt-5">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button type="submit" className={primaryButtonClass} disabled={isSaving}>
            {isSaving ? t("appointments.booking") : t("appointments.bookAppointment")}
          </button>
        </div>
      </form>
    </Modal>
  );
}
