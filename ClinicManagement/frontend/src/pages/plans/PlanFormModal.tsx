import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";
import { PLAN_TIER_LABELS, translatePlanTier } from "../../lib/format";

export interface PlanDto {
  id: string;
  name: string;
  tier: string;
  price: number;
  durationInDays: number;
  maxDoctors: number;
  maxEmployees: number;
  maxPatients: number;
  maxAppointmentsPerMonth: number;
  storageLimitMb: number;
  isActive: boolean;
}

interface PlanFormModalProps {
  plan?: PlanDto;
  onClose: () => void;
  onSaved: () => void;
}

const BLANK_FORM = {
  name: "",
  tier: "Free",
  price: "0",
  durationInDays: "30",
  maxDoctors: "1",
  maxEmployees: "1",
  maxPatients: "100",
  maxAppointmentsPerMonth: "100",
  storageLimitMb: "500",
  isActive: true,
};

export function PlanFormModal({ plan, onClose, onSaved }: PlanFormModalProps) {
  const { t } = useTranslation();
  const isEdit = !!plan;
  const [form, setForm] = useState(BLANK_FORM);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!plan) return;
    setForm({
      name: plan.name,
      tier: plan.tier,
      price: String(plan.price),
      durationInDays: String(plan.durationInDays),
      maxDoctors: String(plan.maxDoctors),
      maxEmployees: String(plan.maxEmployees),
      maxPatients: String(plan.maxPatients),
      maxAppointmentsPerMonth: String(plan.maxAppointmentsPerMonth),
      storageLimitMb: String(plan.storageLimitMb),
      isActive: plan.isActive,
    });
  }, [plan]);

  function set<K extends keyof typeof BLANK_FORM>(key: K, value: (typeof BLANK_FORM)[K]) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setIsSaving(true);
    setError(null);

    const shared = {
      name: form.name,
      price: Number(form.price) || 0,
      durationInDays: Number(form.durationInDays) || 30,
      maxDoctors: Number(form.maxDoctors) || 0,
      maxEmployees: Number(form.maxEmployees) || 0,
      maxPatients: Number(form.maxPatients) || 0,
      maxAppointmentsPerMonth: Number(form.maxAppointmentsPerMonth) || 0,
      storageLimitMb: Number(form.storageLimitMb) || 0,
    };

    try {
      if (isEdit) {
        await api.put(`/api/plans/${plan.id}`, { id: plan.id, ...shared, isActive: form.isActive });
      } else {
        await api.post("/api/plans", { ...shared, tier: form.tier });
      }
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("plans.failedToSave"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      title={isEdit ? t("plans.editPlan") : t("plans.newPlan")}
      subtitle={isEdit ? t("plans.updatePricingLimits") : t("plans.addPlanSubtitle")}
      onClose={onClose}
      width="xl"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className={labelClass}>{t("common.name")}</label>
            <input
              className={inputClass}
              value={form.name}
              onChange={(e) => set("name", e.target.value)}
              required
              maxLength={100}
            />
          </div>
          {!isEdit && (
            <div>
              <label className={labelClass}>{t("plans.tier")}</label>
              <select className={inputClass} value={form.tier} onChange={(e) => set("tier", e.target.value)}>
                {Object.keys(PLAN_TIER_LABELS).map((value) => (
                  <option key={value} value={value}>
                    {translatePlanTier(value, t)}
                  </option>
                ))}
              </select>
            </div>
          )}
          <div>
            <label className={labelClass}>{t("treatmentPackages.price")}</label>
            <input
              type="number"
              min={0}
              step="0.01"
              className={inputClass}
              value={form.price}
              onChange={(e) => set("price", e.target.value)}
              required
            />
          </div>
          <div>
            <label className={labelClass}>{t("plans.durationDaysLabel")}</label>
            <input
              type="number"
              min={1}
              className={inputClass}
              value={form.durationInDays}
              onChange={(e) => set("durationInDays", e.target.value)}
              required
            />
          </div>
          <div>
            <label className={labelClass}>{t("plans.maxDoctorsLabel")}</label>
            <input
              type="number"
              min={0}
              className={inputClass}
              value={form.maxDoctors}
              onChange={(e) => set("maxDoctors", e.target.value)}
              required
            />
          </div>
          <div>
            <label className={labelClass}>{t("plans.maxEmployeesLabel")}</label>
            <input
              type="number"
              min={0}
              className={inputClass}
              value={form.maxEmployees}
              onChange={(e) => set("maxEmployees", e.target.value)}
              required
            />
          </div>
          <div>
            <label className={labelClass}>{t("plans.maxPatientsLabel")}</label>
            <input
              type="number"
              min={0}
              className={inputClass}
              value={form.maxPatients}
              onChange={(e) => set("maxPatients", e.target.value)}
              required
            />
          </div>
          <div>
            <label className={labelClass}>{t("plans.maxAppointmentsLabel")}</label>
            <input
              type="number"
              min={0}
              className={inputClass}
              value={form.maxAppointmentsPerMonth}
              onChange={(e) => set("maxAppointmentsPerMonth", e.target.value)}
              required
            />
          </div>
          <div>
            <label className={labelClass}>{t("plans.storageLimitLabel")}</label>
            <input
              type="number"
              min={0}
              className={inputClass}
              value={form.storageLimitMb}
              onChange={(e) => set("storageLimitMb", e.target.value)}
              required
            />
          </div>
        </div>

        {isEdit && (
          <label className="flex items-center gap-2 text-sm text-ink">
            <input type="checkbox" checked={form.isActive} onChange={(e) => set("isActive", e.target.checked)} />
            {t("plans.activeVisibleHint")}
          </label>
        )}

        <div className="flex justify-end gap-2 border-t border-line pt-5">
          <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
            {t("common.cancel")}
          </button>
          <button type="submit" className={primaryButtonClass} disabled={isSaving}>
            {isSaving ? t("common.saving") : isEdit ? t("fields.saveChanges") : t("plans.createPlan")}
          </button>
        </div>
      </form>
    </Modal>
  );
}
