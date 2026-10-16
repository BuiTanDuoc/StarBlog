import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useApiData } from "../../lib/useApiData";
import { api, ApiError } from "../../lib/api";
import { formatDate, formatCurrency } from "../../lib/format";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";
import { Modal } from "../../components/Modal";

interface PatientPackage {
  id: string;
  treatmentPackageName: string;
  sessionsTotal: number;
  sessionsUsed: number;
  sessionsRemaining: number;
  purchasedAt: string;
  expiresAt: string;
  isExpired: boolean;
}

interface TreatmentPackageOption {
  id: string;
  name: string;
  sessionsIncluded: number;
  price: number;
}

function SellPackageModal({
  patientId,
  onClose,
  onSold,
}: {
  patientId: string;
  onClose: () => void;
  onSold: () => void;
}) {
  const { t } = useTranslation();
  const { data: catalog } = useApiData<TreatmentPackageOption[]>("/api/treatmentpackages?activeOnly=true");
  const [packageId, setPackageId] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const selected = (catalog ?? []).find((p) => p.id === packageId);

  async function handleSell() {
    if (!packageId) return;
    setIsSaving(true);
    setError(null);
    try {
      await api.post("/api/patientpackages", { patientId, treatmentPackageId: packageId, dueDate: null });
      onSold();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("treatmentPackages.couldNotSell"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      title={t("treatmentPackages.sellPackage")}
      subtitle={t("treatmentPackages.sellPackageSubtitle")}
      onClose={onClose}
      width="md"
    >
      <div className="space-y-4">
        <div>
          <label className={labelClass}>{t("treatmentPackages.package")}</label>
          <select className={inputClass} value={packageId} onChange={(e) => setPackageId(e.target.value)}>
            <option value="">{t("treatmentPackages.selectPackage")}</option>
            {(catalog ?? []).map((p) => (
              <option key={p.id} value={p.id}>
                {p.name} — {t("treatmentPackages.sessionsCount", { count: p.sessionsIncluded })} — {formatCurrency(p.price)}
              </option>
            ))}
          </select>
        </div>

        {selected && (
          <p className="text-sm text-ink-muted">
            {t("treatmentPackages.sellPackageExplainer", {
              price: formatCurrency(selected.price),
              count: selected.sessionsIncluded,
            })}
          </p>
        )}

        {error && <p className="text-sm text-danger">{error}</p>}

        <div className="flex justify-end gap-2 border-t border-line pt-4">
          <button className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button className={primaryButtonClass} onClick={handleSell} disabled={!packageId || isSaving}>
            {isSaving ? t("treatmentPackages.selling") : t("treatmentPackages.sellPackage")}
          </button>
        </div>
      </div>
    </Modal>
  );
}

export function PatientPackagesPanel({ patientId, canSell }: { patientId: string; canSell: boolean }) {
  const { t } = useTranslation();
  const { data, isLoading, refetch } = useApiData<PatientPackage[]>(
    `/api/patientpackages?patientId=${patientId}`,
  );
  const [showSell, setShowSell] = useState(false);

  return (
    <div className="space-y-3">
      {canSell && (
        <div className="flex justify-end">
          <button className={primaryButtonClass} onClick={() => setShowSell(true)}>
            + {t("treatmentPackages.sellPackage")}
          </button>
        </div>
      )}

      <div className="overflow-hidden rounded-sm border border-line bg-surface">
        {isLoading && <p className="px-5 py-6 text-sm text-ink-muted">{t("common.loading")}</p>}
        {!isLoading && (data ?? []).length === 0 && (
          <p className="px-5 py-6 text-sm text-ink-faint">{t("treatmentPackages.noPackagesYet")}</p>
        )}
        {(data ?? []).map((p) => (
          <div key={p.id} className="flex items-center justify-between border-b border-line px-5 py-4 last:border-0">
            <div>
              <p className="text-sm font-medium text-ink">{p.treatmentPackageName}</p>
              <p className="text-xs text-ink-faint">
                {t("treatmentPackages.bought")} {formatDate(p.purchasedAt)} · {t("treatmentPackages.expires")} {formatDate(p.expiresAt)}
                {p.isExpired && <span className="ms-1 text-danger">· {t("enums.subscriptionStatus.Expired")}</span>}
              </p>
            </div>
            <div className="text-end">
              <p className="text-sm text-ink">
                {t("treatmentPackages.sessionsLeft", { remaining: p.sessionsRemaining, total: p.sessionsTotal })}
              </p>
            </div>
          </div>
        ))}
      </div>

      {showSell && (
        <SellPackageModal
          patientId={patientId}
          onClose={() => setShowSell(false)}
          onSold={() => {
            setShowSell(false);
            refetch();
          }}
        />
      )}
    </div>
  );
}
