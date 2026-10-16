import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { api, ApiError } from "../../lib/api";
import { formatTime } from "../../lib/format";
import { labelClass } from "../../lib/ui";

interface Slot {
  start: string;
  end: string;
}

interface DoctorSlotPickerProps {
  doctorId: string;
  date: string;
  selected: Slot | null;
  onSelect: (slot: Slot) => void;
}

export function DoctorSlotPicker({ doctorId, date, selected, onSelect }: DoctorSlotPickerProps) {
  const { t } = useTranslation();
  const [slots, setSlots] = useState<Slot[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!doctorId || !date) {
      setSlots([]);
      return;
    }
    setIsLoading(true);
    setError(null);
    api
      .get<Slot[]>(`/api/appointments/availability?doctorId=${doctorId}&date=${date}&slotMinutes=30`)
      .then(setSlots)
      .catch((err) => setError(err instanceof ApiError ? err.message : t("appointments.failedToLoadAvailability")))
      .finally(() => setIsLoading(false));
  }, [doctorId, date]);

  if (!doctorId || !date) {
    return <p className="text-sm text-ink-faint">{t("appointments.chooseDoctorAndDate")}</p>;
  }

  return (
    <div>
      <label className={labelClass}>{t("appointments.availableTimes")}</label>
      {isLoading && <p className="text-sm text-ink-muted">{t("appointments.loadingSlots")}</p>}
      {error && <p className="text-sm text-danger">{error}</p>}
      {!isLoading && !error && slots.length === 0 && (
        <p className="text-sm text-ink-faint">{t("appointments.noOpenSlots")}</p>
      )}
      {slots.length > 0 && (
        <div className="flex max-h-40 flex-wrap gap-2 overflow-y-auto">
          {slots.map((slot) => {
            const isSelected = selected?.start === slot.start;
            return (
              <button
                key={slot.start}
                type="button"
                onClick={() => onSelect(slot)}
                className={`rounded-sm border px-2.5 py-1.5 text-xs font-medium tabular-nums transition-colors ${
                  isSelected
                    ? "border-harbor-500 bg-harbor-500 text-white"
                    : "border-line bg-surface text-ink hover:border-harbor-400"
                }`}
              >
                {formatTime(slot.start)}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}
