import { useState } from "react";
import { useTranslation } from "react-i18next";
import { PortalShell } from "./PortalShell";
import { useApiData } from "../../lib/useApiData";
import { formatDate } from "../../lib/format";

interface PortalMedicalRecord {
  id: string;
  doctorName: string;
  diagnosis: string;
  symptoms?: string | null;
  treatmentPlan?: string | null;
  notes?: string | null;
  followUpDate?: string | null;
  templateName?: string | null;
  customFieldsJson?: string | null;
  createdAt: string;
}

function parseCustomFields(json?: string | null): Record<string, unknown> {
  if (!json) return {};
  try {
    return JSON.parse(json);
  } catch {
    return {};
  }
}

export function PortalMedicalRecordsPage() {
  const { t } = useTranslation();
  const { data, isLoading } = useApiData<PortalMedicalRecord[]>("/api/patient-portal/medical-records");
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const records = [...(data ?? [])].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
  );

  return (
    <PortalShell title={t("medicalRecords.tabRecords")}>
      <p className="mb-5 text-sm text-ink-muted">{t("portal.readOnlyRecordsHint")}</p>

      {isLoading && <p className="text-sm text-ink-muted">{t("common.loading")}</p>}
      {!isLoading && records.length === 0 && <p className="text-sm text-ink-muted">{t("portal.noRecordsYet")}</p>}

      <div className="space-y-3">
        {records.map((r) => {
          const isExpanded = expandedId === r.id;
          const customFields = parseCustomFields(r.customFieldsJson);
          return (
            <div key={r.id} className="rounded-sm border border-line bg-surface">
              <button
                className="flex w-full items-center justify-between px-5 py-4 text-start"
                onClick={() => setExpandedId(isExpanded ? null : r.id)}
              >
                <div>
                  <p className="text-sm font-medium text-ink">{r.diagnosis}</p>
                  <p className="text-xs text-ink-faint">
                    {formatDate(r.createdAt)} · {t("medicalRecords.drName", { name: r.doctorName })}
                    {r.templateName && ` · ${r.templateName}`}
                  </p>
                </div>
                <span className="text-ink-faint">{isExpanded ? "−" : "+"}</span>
              </button>

              {isExpanded && (
                <div className="space-y-2 border-t border-line px-5 py-4 text-sm">
                  {r.symptoms && (
                    <p><span className="font-medium text-ink">{t("medicalRecords.symptomsLabel")} </span><span className="text-ink-muted">{r.symptoms}</span></p>
                  )}
                  {r.treatmentPlan && (
                    <p><span className="font-medium text-ink">{t("medicalRecords.treatmentPlan")}: </span><span className="text-ink-muted">{r.treatmentPlan}</span></p>
                  )}
                  {r.notes && (
                    <p><span className="font-medium text-ink">{t("common.notes")}: </span><span className="text-ink-muted">{r.notes}</span></p>
                  )}
                  {r.followUpDate && (
                    <p><span className="font-medium text-ink">{t("medicalRecords.followUp")}: </span><span className="text-ink-muted">{formatDate(r.followUpDate)}</span></p>
                  )}
                  {Object.keys(customFields).length > 0 && (
                    <div>
                      <p className="font-medium text-ink">{r.templateName ?? t("portal.additionalDetails")}</p>
                      <dl className="mt-1 grid grid-cols-2 gap-x-4 gap-y-1">
                        {Object.entries(customFields).map(([key, value]) => (
                          <div key={key} className="contents">
                            <dt className="text-ink-faint">{key}</dt>
                            <dd className="text-ink-muted">{String(value)}</dd>
                          </div>
                        ))}
                      </dl>
                    </div>
                  )}
                </div>
              )}
            </div>
          );
        })}
      </div>
    </PortalShell>
  );
}
