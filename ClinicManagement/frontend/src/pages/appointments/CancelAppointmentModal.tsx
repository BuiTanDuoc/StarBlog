import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import { dangerButtonClass, inputClass, labelClass, secondaryButtonClass } from "../../lib/ui";

interface CancelAppointmentModalProps {
  appointmentId: string;
  patientName: string;
  onClose: () => void;
  onCancelled: () => void;
}

export function CancelAppointmentModal({
  appointmentId,
  patientName,
  onClose,
  onCancelled,
}: CancelAppointmentModalProps) {
  const { t } = useTranslation();
  const [reason, setReason] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!reason.trim()) {
      setError(t("appointments.reasonRequired"));
      return;
    }
    setIsSaving(true);
    setError(null);
    try {
      await api.put(`/api/appointments/${appointmentId}/cancel`, { reason: reason.trim() });
      onCancelled();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("appointments.failedToCancel"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal title={t("appointments.cancelAppointment")} subtitle={t("appointments.forPatient", { name: patientName })} onClose={onClose} width="md">
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}
        <div>
          <label className={labelClass}>{t("appointments.reason")}</label>
          <textarea
            className={inputClass}
            rows={3}
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder={t("appointments.reasonPlaceholder")}
            autoFocus
          />
        </div>
        <div className="flex justify-end gap-2">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("appointments.keepAppointment")}
          </button>
          <button type="submit" className={dangerButtonClass} disabled={isSaving}>
            {isSaving ? t("appointments.cancelling") : t("appointments.cancelAppointment")}
          </button>
        </div>
      </form>
    </Modal>
  );
}
