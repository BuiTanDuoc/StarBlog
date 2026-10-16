import { useTranslation } from "react-i18next";
import { PortalShell } from "./PortalShell";
import { useApiData } from "../../lib/useApiData";
import { formatDate, translateGender, translateBloodGroup } from "../../lib/format";

interface PortalProfile {
  patientCode: string;
  fullName: string;
  gender: string;
  dateOfBirth: string;
  phone: string;
  email?: string | null;
  address?: string | null;
  bloodGroup: string;
  allergies?: string | null;
  age: number;
}

function Field({ label, value }: { label: string; value?: string | null }) {
  return (
    <div>
      <dt className="text-xs uppercase tracking-[0.06em] text-ink-faint">{label}</dt>
      <dd className="mt-0.5 text-sm text-ink">{value || "—"}</dd>
    </div>
  );
}

export function PortalProfilePage() {
  const { t } = useTranslation();
  const { data, isLoading } = useApiData<PortalProfile>("/api/patient-portal/profile");

  return (
    <PortalShell title={t("portal.myProfile")}>
      {isLoading && <p className="text-sm text-ink-muted">{t("common.loading")}</p>}
      {data && (
        <div className="max-w-lg rounded-sm border border-line bg-surface p-6">
          <dl className="grid grid-cols-2 gap-5">
            <Field label={t("patients.code")} value={data.patientCode} />
            <Field label={t("fields.fullName")} value={data.fullName} />
            <Field label={t("patients.gender")} value={translateGender(data.gender, t)} />
            <Field label={t("fields.dateOfBirth")} value={`${formatDate(data.dateOfBirth)} (${data.age})`} />
            <Field label={t("common.phone")} value={data.phone} />
            <Field label={t("common.email")} value={data.email} />
            <Field label={t("fields.bloodGroup")} value={translateBloodGroup(data.bloodGroup, t)} />
            <Field label={t("common.address")} value={data.address} />
          </dl>
          {data.allergies && (
            <div className="mt-5 border-t border-line pt-5">
              <Field label={t("fields.allergies")} value={data.allergies} />
            </div>
          )}
          <p className="mt-6 text-xs text-ink-faint">
            {t("portal.updateDetailsContactClinic")}
          </p>
        </div>
      )}
    </PortalShell>
  );
}
