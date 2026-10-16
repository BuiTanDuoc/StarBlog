import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { MedicalRecordFormModal, type MedicalRecordDto } from "./MedicalRecordFormModal";
import { PrescriptionFormModal } from "./PrescriptionFormModal";
import { AttachmentsPanel } from "./AttachmentsPanel";
import { PatientPackagesPanel } from "./PatientPackagesPanel";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import { ApiError, downloadFile } from "../../lib/api";
import { calculateAge, formatDate, formatDateTime, translateGender } from "../../lib/format";
import { primaryButtonClass, secondaryButtonClass } from "../../lib/ui";

interface PatientDetail {
  id: string;
  patientCode: string;
  fullName: string;
  gender: string;
  dateOfBirth: string;
}

function CustomFieldsSummary({ json }: { json: string }) {
  let fields: Record<string, unknown>;
  try {
    fields = JSON.parse(json);
  } catch {
    return null;
  }
  const entries = Object.entries(fields).filter(([, v]) => v !== "" && v != null);
  if (entries.length === 0) return null;

  return (
    <dl className="mt-2 grid grid-cols-2 gap-x-4 gap-y-1 border-t border-line pt-2 text-sm sm:grid-cols-3">
      {entries.map(([key, value]) => (
        <div key={key} className="contents">
          <dt className="text-ink-faint">{key}</dt>
          <dd className="text-ink-muted">{String(value)}</dd>
        </div>
      ))}
    </dl>
  );
}

interface PrescriptionItem {
  medicineName: string;
  dosage: string;
  frequency: string;
  duration: string;
  instructions?: string | null;
}

interface PrescriptionDto {
  id: string;
  doctorName: string;
  dateIssued: string;
  items: PrescriptionItem[];
}

type Tab = "records" | "prescriptions" | "files" | "packages";

export function PatientChartPage() {
  const { t } = useTranslation();
  const { patientId } = useParams<{ patientId: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const roles = user?.roles ?? [];
  const canCreate = roles.some((r) => ["ClinicOwner", "Doctor", "Nurse", "SuperAdmin"].includes(r));
  const canEditRecord = roles.some((r) => ["ClinicOwner", "Doctor", "SuperAdmin"].includes(r));
  const canSellPackages = roles.some((r) => ["ClinicOwner", "Receptionist", "SuperAdmin"].includes(r));

  const [tab, setTab] = useState<Tab>("records");
  const [recordFormState, setRecordFormState] = useState<
    { mode: "create" } | { mode: "edit"; record: MedicalRecordDto } | null
  >(null);
  const [prescriptionFor, setPrescriptionFor] = useState<MedicalRecordDto | null>(null);
  const [downloadError, setDownloadError] = useState<string | null>(null);

  const { data: patient } = useApiData<PatientDetail>(patientId ? `/api/patients/${patientId}` : "");
  const {
    data: records,
    isLoading: recordsLoading,
    error: recordsError,
    refetch: refetchRecords,
  } = useApiData<MedicalRecordDto[]>(patientId ? `/api/medicalrecords/by-patient/${patientId}` : "");
  const {
    data: prescriptions,
    isLoading: prescriptionsLoading,
    error: prescriptionsError,
    refetch: refetchPrescriptions,
  } = useApiData<PrescriptionDto[]>(patientId ? `/api/prescriptions/by-patient/${patientId}` : "");

  async function handleDownloadPdf(id: string) {
    setDownloadError(null);
    try {
      await downloadFile(`/api/prescriptions/${id}/pdf`, `Prescription-${id}.pdf`);
    } catch (err) {
      setDownloadError(err instanceof ApiError ? err.message : t("medicalRecords.failedToDownloadPdf"));
    }
  }

  if (!patientId) return null;

  return (
    <AppShell title={t("medicalRecords.patientChart")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-5">
        <div className="flex items-center justify-between">
          <div>
            <Link to="/patients" className="text-xs font-medium text-harbor-500 hover:underline">
              ← {t("medicalRecords.backToPatients")}
            </Link>
            {patient && (
              <h2 className="mt-1 font-display text-xl text-ink">
                {patient.fullName}{" "}
                <span className="font-sans text-sm font-normal text-ink-faint">
                  · {patient.patientCode} · {translateGender(patient.gender, t)} ·{" "}
                  {t("medicalRecords.yearsOld", { age: calculateAge(patient.dateOfBirth) })}
                </span>
              </h2>
            )}
          </div>
          <button className={secondaryButtonClass} onClick={() => navigate("/patients")}>
            {t("medicalRecords.closeChart")}
          </button>
        </div>

        <div className="flex gap-2 border-b border-line">
          {(["records", "prescriptions", "files", "packages"] as Tab[]).map((tt) => (
            <button
              key={tt}
              onClick={() => setTab(tt)}
              className={`-mb-px border-b-2 px-3 py-2 text-sm font-medium transition-colors ${
                tab === tt ? "border-harbor-500 text-harbor-500" : "border-transparent text-ink-faint hover:text-ink"
              }`}
            >
              {tt === "records"
                ? t("medicalRecords.tabRecords")
                : tt === "prescriptions"
                  ? t("medicalRecords.tabPrescriptions")
                  : tt === "files"
                    ? t("medicalRecords.tabFiles")
                    : t("medicalRecords.tabPackages")}
            </button>
          ))}
        </div>

        {tab === "records" && (
          <div className="space-y-3">
            {canCreate && (
              <div className="flex justify-end">
                <button className={primaryButtonClass} onClick={() => setRecordFormState({ mode: "create" })}>
                  + {t("medicalRecords.addRecord")}
                </button>
              </div>
            )}

            {recordsLoading && (
              <p className="px-1 py-8 text-center font-mono text-sm text-ink-muted">{t("medicalRecords.loadingRecords")}</p>
            )}
            {recordsError && <p className="text-sm text-danger">{recordsError}</p>}
            {records && records.length === 0 && !recordsLoading && (
              <p className="py-10 text-center text-sm text-ink-faint">{t("medicalRecords.noRecordsYet")}</p>
            )}

            {records?.map((r) => (
              <div key={r.id} className="rounded-lg border border-line bg-surface p-4 shadow-card">
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <p className="font-medium text-ink">{r.diagnosis}</p>
                    <p className="mt-0.5 text-xs text-ink-faint">
                      {t("medicalRecords.drName", { name: r.doctorName })} · {formatDateTime(r.createdAt)}
                      {r.followUpDate && <> · {t("medicalRecords.followUp")} {formatDate(r.followUpDate)}</>}
                      {r.templateName && <> · {r.templateName}</>}
                    </p>
                  </div>
                  <div className="flex shrink-0 gap-3">
                    {canCreate && (
                      <button
                        className="text-xs font-medium text-harbor-500 hover:underline"
                        onClick={() => setPrescriptionFor(r)}
                      >
                        + {t("medicalRecords.prescription")}
                      </button>
                    )}
                    {canEditRecord && (
                      <button
                        className="text-xs font-medium text-harbor-500 hover:underline"
                        onClick={() => setRecordFormState({ mode: "edit", record: r })}
                      >
                        {t("common.edit")}
                      </button>
                    )}
                  </div>
                </div>
                {r.symptoms && <p className="mt-2 text-sm text-ink-muted">{t("medicalRecords.symptomsLabel")} {r.symptoms}</p>}
                {r.treatmentPlan && <p className="mt-1 text-sm text-ink-muted">{t("medicalRecords.treatmentLabel")} {r.treatmentPlan}</p>}
                {r.notes && <p className="mt-1 text-sm text-ink-faint">{r.notes}</p>}
                {r.customFieldsJson && <CustomFieldsSummary json={r.customFieldsJson} />}
              </div>
            ))}
          </div>
        )}

        {tab === "prescriptions" && (
          <div className="space-y-3">
            {downloadError && <p className="text-sm text-danger">{downloadError}</p>}
            {prescriptionsLoading && (
              <p className="px-1 py-8 text-center font-mono text-sm text-ink-muted">{t("medicalRecords.loadingPrescriptions")}</p>
            )}
            {prescriptionsError && <p className="text-sm text-danger">{prescriptionsError}</p>}
            {prescriptions && prescriptions.length === 0 && !prescriptionsLoading && (
              <p className="py-10 text-center text-sm text-ink-faint">{t("medicalRecords.noPrescriptionsYet")}</p>
            )}

            {prescriptions?.map((p) => (
              <div key={p.id} className="rounded-lg border border-line bg-surface p-4 shadow-card">
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <p className="text-xs text-ink-faint">
                    {t("medicalRecords.drName", { name: p.doctorName })} · {formatDateTime(p.dateIssued)}
                  </p>
                  <button
                    className="text-xs font-medium text-harbor-500 hover:underline"
                    onClick={() => handleDownloadPdf(p.id)}
                  >
                    {t("medicalRecords.downloadPdf")}
                  </button>
                </div>
                <ul className="mt-2 space-y-1 text-sm">
                  {p.items.map((item, i) => (
                    <li key={i} className="text-ink-muted">
                      <span className="font-medium text-ink">{item.medicineName}</span> — {item.dosage},{" "}
                      {item.frequency}, {item.duration}
                      {item.instructions && <span className="text-ink-faint"> ({item.instructions})</span>}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        )}
        {tab === "files" && <AttachmentsPanel entityName="Patient" entityId={patientId} canManage={canCreate} />}

        {tab === "packages" && <PatientPackagesPanel patientId={patientId} canSell={canSellPackages} />}
      </div>

      {recordFormState && (
        <MedicalRecordFormModal
          patientId={patientId}
          record={recordFormState.mode === "edit" ? recordFormState.record : undefined}
          onClose={() => setRecordFormState(null)}
          onSaved={() => {
            setRecordFormState(null);
            refetchRecords();
          }}
        />
      )}

      {prescriptionFor && (
        <PrescriptionFormModal
          patientId={patientId}
          doctorId={prescriptionFor.doctorId}
          medicalRecordId={prescriptionFor.id}
          onClose={() => setPrescriptionFor(null)}
          onSaved={() => {
            setPrescriptionFor(null);
            refetchPrescriptions();
            setTab("prescriptions");
          }}
        />
      )}
    </AppShell>
  );
}
