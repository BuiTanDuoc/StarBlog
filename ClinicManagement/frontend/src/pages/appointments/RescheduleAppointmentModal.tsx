import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { DoctorSlotPicker } from "./DoctorSlotPicker";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";
import { toDateInputValue } from "../../lib/format";

interface Slot {
  start: string;
  end: string;
}

interface RescheduleAppointmentModalProps {
  appointmentId: string;
  doctorId: string;
  patientName: string;
  currentStart: string;
  onClose: () => void;
  onRescheduled: () => void;
}

export function RescheduleAppointmentModal({
  appointmentId,
  doctorId,
  patientName,
  currentStart,
  onClose,
  onRescheduled,
}: RescheduleAppointmentModalProps) {
  const { t } = useTranslation();
  const [date, setDate] = useState(toDateInputValue(currentStart));
  const [slot, setSlot] = useState<Slot | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const todayIso = new Date().toISOString().slice(0, 10);

  async function handleSubmit() {
    if (!slot) {
      setError(t("appointments.chooseNewSlot"));
      return;
    }
    setIsSaving(true);
    setError(null);
    try {
      await api.put(`/api/appointments/${appointmentId}/reschedule`, {
        newStart: slot.start,
        newEnd: slot.end,
      });
      onRescheduled();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("appointments.failedToReschedule"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal title={t("appointments.rescheduleAppointment")} subtitle={t("appointments.forPatient", { name: patientName })} onClose={onClose} width="lg">
      <div className="space-y-4">
        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}
        <div>
          <label className={labelClass}>{t("appointments.newDate")}</label>
          <input
            type="date"
            className={inputClass}
            value={date}
            min={todayIso}
            onChange={(e) => {
              setDate(e.target.value);
              setSlot(null);
            }}
          />
        </div>
        <DoctorSlotPicker doctorId={doctorId} date={date} selected={slot} onSelect={setSlot} />
        <div className="flex justify-end gap-2 border-t border-line pt-5">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button type="button" className={primaryButtonClass} onClick={handleSubmit} disabled={isSaving}>
            {isSaving ? t("common.saving") : t("appointments.confirmNewTime")}
          </button>
        </div>
      </div>
    </Modal>
  );
}
