import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";
import { WEEKDAYS, translateWeekday } from "../../lib/format";

interface Branch {
  id: string;
  name: string;
}

interface ScheduleSlot {
  Day: string;
  Start: string;
  End: string;
}

interface DoctorDetail {
  id: string;
  fullName: string;
  specialization: string;
  licenseNumber: string;
  consultationFee: number;
  workScheduleJson: string;
  yearsOfExperience: number;
  status: string;
  branchId: string;
}

interface DoctorFormModalProps {
  doctorId?: string;
  onClose: () => void;
  onSaved: () => void;
}

const BLANK_FORM = {
  fullName: "",
  specialization: "",
  licenseNumber: "",
  consultationFee: "",
  yearsOfExperience: "",
  status: "Active",
  branchId: "",
  applicationUserId: "",
};

export function DoctorFormModal({ doctorId, onClose, onSaved }: DoctorFormModalProps) {
  const { t } = useTranslation();
  const isEdit = !!doctorId;
  const [form, setForm] = useState(BLANK_FORM);
  const [schedule, setSchedule] = useState<Record<string, { enabled: boolean; start: string; end: string }>>(
    Object.fromEntries(WEEKDAYS.map((d) => [d, { enabled: false, start: "09:00", end: "17:00" }])),
  );
  const [branches, setBranches] = useState<Branch[]>([]);
  const [isLoading, setIsLoading] = useState(isEdit);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.get<Branch[]>("/api/branches?activeOnly=true").then(setBranches).catch(() => setBranches([]));
  }, []);

  useEffect(() => {
    if (!doctorId) return;
    setIsLoading(true);
    api
      .get<DoctorDetail>(`/api/doctors/${doctorId}`)
      .then((d) => {
        setForm({
          fullName: d.fullName,
          specialization: d.specialization,
          licenseNumber: d.licenseNumber,
          consultationFee: String(d.consultationFee),
          yearsOfExperience: String(d.yearsOfExperience),
          status: d.status,
          branchId: d.branchId,
          applicationUserId: "",
        });
        try {
          const slots: ScheduleSlot[] = JSON.parse(d.workScheduleJson || "[]");
          setSchedule((prev) => {
            const next = { ...prev };
            for (const slot of slots) {
              if (next[slot.Day]) next[slot.Day] = { enabled: true, start: slot.Start, end: slot.End };
            }
            return next;
          });
        } catch {
          // malformed schedule JSON — leave the default (all days off)
        }
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : t("doctors.failedToLoad")))
      .finally(() => setIsLoading(false));
  }, [doctorId]);

  function set<K extends keyof typeof BLANK_FORM>(key: K, value: string) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  function toggleDay(day: string, enabled: boolean) {
    setSchedule((s) => ({ ...s, [day]: { ...s[day], enabled } }));
  }

  function setDayTime(day: string, field: "start" | "end", value: string) {
    setSchedule((s) => ({ ...s, [day]: { ...s[day], [field]: value } }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setIsSaving(true);
    setError(null);

    const workScheduleJson = JSON.stringify(
      WEEKDAYS.filter((d) => schedule[d].enabled).map((d) => ({
        Day: d,
        Start: schedule[d].start,
        End: schedule[d].end,
      })),
    );

    const basePayload = {
      fullName: form.fullName,
      specialization: form.specialization,
      consultationFee: Number(form.consultationFee) || 0,
      workScheduleJson,
      yearsOfExperience: Number(form.yearsOfExperience) || 0,
      branchId: form.branchId,
    };

    try {
      if (isEdit) {
        await api.put(`/api/doctors/${doctorId}`, { id: doctorId, ...basePayload, status: form.status });
      } else {
        await api.post("/api/doctors", {
          ...basePayload,
          licenseNumber: form.licenseNumber,
          applicationUserId: form.applicationUserId || null,
        });
      }
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("doctors.failedToSave"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      title={isEdit ? t("doctors.editDoctor") : t("doctors.addDoctor")}
      subtitle={isEdit ? t("doctors.updateProfileSubtitle") : t("doctors.addToRosterSubtitle")}
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
              <label className={labelClass}>{t("doctors.specialization")}</label>
              <input
                className={inputClass}
                value={form.specialization}
                onChange={(e) => set("specialization", e.target.value)}
                required
                maxLength={150}
              />
            </div>
            <div>
              <label className={labelClass}>{t("doctors.licenseNumber")}</label>
              <input
                className={inputClass}
                value={form.licenseNumber}
                onChange={(e) => set("licenseNumber", e.target.value)}
                required
                disabled={isEdit}
                maxLength={100}
              />
            </div>
            {!isEdit && (
              <div>
                <label className={labelClass}>{t("doctors.linkedAccount")}</label>
                <input
                  className={inputClass}
                  value={form.applicationUserId}
                  onChange={(e) => set("applicationUserId", e.target.value)}
                  placeholder={t("doctors.linkedAccountPlaceholder")}
                />
              </div>
            )}
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
            <div>
              <label className={labelClass}>{t("doctors.consultationFee")}</label>
              <input
                type="number"
                min={0}
                step="0.01"
                className={inputClass}
                value={form.consultationFee}
                onChange={(e) => set("consultationFee", e.target.value)}
                required
              />
            </div>
            <div>
              <label className={labelClass}>{t("doctors.yearsOfExperience")}</label>
              <input
                type="number"
                min={0}
                className={inputClass}
                value={form.yearsOfExperience}
                onChange={(e) => set("yearsOfExperience", e.target.value)}
                required
              />
            </div>
            {isEdit && (
              <div>
                <label className={labelClass}>{t("common.status")}</label>
                <select className={inputClass} value={form.status} onChange={(e) => set("status", e.target.value)}>
                  <option value="Active">{t("enums.doctorStatus.Active")}</option>
                  <option value="OnLeave">{t("enums.doctorStatus.OnLeave")}</option>
                  <option value="Suspended">{t("enums.doctorStatus.Suspended")}</option>
                  <option value="Inactive">{t("enums.doctorStatus.Inactive")}</option>
                </select>
              </div>
            )}
          </div>

          <div>
            <label className={labelClass}>{t("doctors.weeklySchedule")}</label>
            <div className="rounded-sm border border-line">
              {WEEKDAYS.map((day, i) => (
                <div
                  key={day}
                  className={`flex flex-wrap items-center gap-3 px-3 py-2.5 ${i !== WEEKDAYS.length - 1 ? "border-b border-line" : ""}`}
                >
                  <label className="flex w-32 items-center gap-2 text-sm text-ink">
                    <input
                      type="checkbox"
                      checked={schedule[day].enabled}
                      onChange={(e) => toggleDay(day, e.target.checked)}
                    />
                    {translateWeekday(day, t)}
                  </label>
                  {schedule[day].enabled ? (
                    <div className="flex items-center gap-2">
                      <input
                        type="time"
                        className="rounded-sm border border-line px-2 py-1 text-sm"
                        value={schedule[day].start}
                        onChange={(e) => setDayTime(day, "start", e.target.value)}
                      />
                      <span className="text-xs text-ink-faint">{t("appointments.to")}</span>
                      <input
                        type="time"
                        className="rounded-sm border border-line px-2 py-1 text-sm"
                        value={schedule[day].end}
                        onChange={(e) => setDayTime(day, "end", e.target.value)}
                      />
                    </div>
                  ) : (
                    <span className="text-xs text-ink-faint">{t("doctors.notWorking")}</span>
                  )}
                </div>
              ))}
            </div>
          </div>

          <div className="flex justify-end gap-2 border-t border-line pt-5">
            <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
              {t("common.cancel")}
            </button>
            <button type="submit" className={primaryButtonClass} disabled={isSaving}>
              {isSaving ? t("common.saving") : isEdit ? t("fields.saveChanges") : t("doctors.addDoctor")}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
