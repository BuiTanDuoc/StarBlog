import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { PortalShell } from "./PortalShell";
import { useApiData } from "../../lib/useApiData";
import { api, ApiError } from "../../lib/api";
import type { PaginatedList } from "../../lib/types";
import { formatTime } from "../../lib/format";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";

interface DoctorOption {
  id: string;
  fullName: string;
  specialization: string;
  consultationFee: number;
  branchId: string;
}

interface AvailableSlot {
  start: string;
  end: string;
}

interface MyPackage {
  id: string;
  treatmentPackageName: string;
  sessionsRemaining: number;
  isExpired: boolean;
}

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

export function PortalBookAppointmentPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { data: doctorsPage, isLoading: doctorsLoading } = useApiData<PaginatedList<DoctorOption>>(
    "/api/patient-portal/doctors?pageNumber=1&pageSize=100",
  );
  const { data: myPackages } = useApiData<MyPackage[]>("/api/patient-portal/packages");
  const usablePackages = (myPackages ?? []).filter((p) => !p.isExpired && p.sessionsRemaining > 0);

  const [doctorId, setDoctorId] = useState("");
  const [date, setDate] = useState(todayIso());
  const [selectedSlot, setSelectedSlot] = useState<AvailableSlot | null>(null);
  const [notes, setNotes] = useState("");
  const [patientPackageId, setPatientPackageId] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isBooking, setIsBooking] = useState(false);

  const { data: slots, isLoading: slotsLoading } = useApiData<AvailableSlot[]>(
    doctorId ? `/api/patient-portal/doctors/${doctorId}/availability?date=${date}&slotMinutes=30` : "",
  );

  const selectedDoctor = (doctorsPage?.items ?? []).find((d) => d.id === doctorId) ?? null;

  async function handleBook() {
    if (!selectedDoctor || !selectedSlot) return;
    setError(null);
    setIsBooking(true);
    try {
      await api.post("/api/patient-portal/appointments", {
        doctorId: selectedDoctor.id,
        branchId: selectedDoctor.branchId,
        scheduledStart: selectedSlot.start,
        scheduledEnd: selectedSlot.end,
        notes,
        patientPackageId: patientPackageId || null,
      });
      navigate("/portal/appointments");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("portal.couldNotBook"));
    } finally {
      setIsBooking(false);
    }
  }

  return (
    <PortalShell title={t("portal.bookAnAppointment")}>
      <div className="max-w-lg space-y-5">
        <div>
          <label className={labelClass}>{t("appointments.doctor")}</label>
          <select
            className={inputClass}
            value={doctorId}
            onChange={(e) => {
              setDoctorId(e.target.value);
              setSelectedSlot(null);
            }}
          >
            <option value="">{doctorsLoading ? t("common.loading") : t("appointments.selectDoctor")}</option>
            {(doctorsPage?.items ?? []).map((d) => (
              <option key={d.id} value={d.id}>
                {d.fullName} — {d.specialization} ({d.consultationFee} EGP)
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
            min={todayIso()}
            onChange={(e) => {
              setDate(e.target.value);
              setSelectedSlot(null);
            }}
          />
        </div>

        {doctorId && (
          <div>
            <label className={labelClass}>{t("appointments.availableTimes")}</label>
            {slotsLoading && <p className="text-sm text-ink-muted">{t("portal.loadingAvailableTimes")}</p>}
            {!slotsLoading && (slots ?? []).length === 0 && (
              <p className="text-sm text-ink-muted">{t("portal.noAvailableSlotsDay")}</p>
            )}
            <div className="flex flex-wrap gap-2">
              {(slots ?? []).map((slot) => (
                <button
                  key={slot.start}
                  type="button"
                  onClick={() => setSelectedSlot(slot)}
                  className={
                    selectedSlot?.start === slot.start
                      ? `${primaryButtonClass} !px-3 !py-1.5 text-xs`
                      : `${secondaryButtonClass} !px-3 !py-1.5 text-xs`
                  }
                >
                  {formatTime(slot.start)}
                </button>
              ))}
            </div>
          </div>
        )}

        <div>
          <label className={labelClass}>{t("appointments.notesOptional")}</label>
          <textarea
            className={inputClass}
            rows={3}
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder={t("portal.reasonForVisitPlaceholder")}
          />
        </div>

        {usablePackages.length > 0 && (
          <div>
            <label className={labelClass}>{t("portal.usePackageSessionOptional")}</label>
            <select className={inputClass} value={patientPackageId} onChange={(e) => setPatientPackageId(e.target.value)}>
              <option value="">{t("portal.dontUsePackage")}</option>
              {usablePackages.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.treatmentPackageName} ({t("portal.sessionsLeftShort", { count: p.sessionsRemaining })})
                </option>
              ))}
            </select>
          </div>
        )}

        {error && <p role="alert" className="rounded-sm bg-danger-light px-3.5 py-2.5 text-sm text-danger">{error}</p>}

        <button
          type="button"
          disabled={!selectedSlot || isBooking}
          onClick={handleBook}
          className={primaryButtonClass}
        >
          {isBooking ? t("appointments.booking") : t("portal.confirmAppointment")}
        </button>
      </div>
    </PortalShell>
  );
}
