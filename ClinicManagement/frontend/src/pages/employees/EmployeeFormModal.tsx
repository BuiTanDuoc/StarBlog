import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../../components/Modal";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass, secondaryButtonClass } from "../../lib/ui";
import { EMPLOYEE_STATUS_STYLES, translateEmployeeStatus, toDateInputValue } from "../../lib/format";

interface Branch {
  id: string;
  name: string;
}

interface Department {
  id: string;
  name: string;
  branchId: string;
}

interface EmployeeDetail {
  id: string;
  fullName: string;
  position: string;
  hireDate: string;
  salary: number;
  status: string;
  branchId: string;
  departmentId?: string | null;
}

interface EmployeeFormModalProps {
  employeeId?: string;
  onClose: () => void;
  onSaved: () => void;
}

const BLANK_FORM = {
  fullName: "",
  position: "",
  hireDate: new Date().toISOString().slice(0, 10),
  salary: "",
  status: "Active",
  branchId: "",
  departmentId: "",
  applicationUserId: "",
};

export function EmployeeFormModal({ employeeId, onClose, onSaved }: EmployeeFormModalProps) {
  const { t } = useTranslation();
  const isEdit = !!employeeId;
  const [form, setForm] = useState(BLANK_FORM);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [isLoading, setIsLoading] = useState(isEdit);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.get<Branch[]>("/api/branches?activeOnly=true").then(setBranches).catch(() => setBranches([]));
  }, []);

  useEffect(() => {
    if (!form.branchId) {
      setDepartments([]);
      return;
    }
    api
      .get<Department[]>(`/api/departments?branchId=${form.branchId}`)
      .then(setDepartments)
      .catch(() => setDepartments([]));
  }, [form.branchId]);

  useEffect(() => {
    if (!employeeId) return;
    setIsLoading(true);
    api
      .get<EmployeeDetail>(`/api/employees/${employeeId}`)
      .then((e) => {
        setForm({
          fullName: e.fullName,
          position: e.position,
          hireDate: toDateInputValue(e.hireDate),
          salary: String(e.salary),
          status: e.status,
          branchId: e.branchId,
          departmentId: e.departmentId ?? "",
          applicationUserId: "",
        });
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : t("employees.failedToLoad")))
      .finally(() => setIsLoading(false));
  }, [employeeId]);

  function set<K extends keyof typeof BLANK_FORM>(key: K, value: string) {
    setForm((f) => ({ ...f, [key]: value, ...(key === "branchId" ? { departmentId: "" } : {}) }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setIsSaving(true);
    setError(null);

    const basePayload = {
      fullName: form.fullName,
      position: form.position,
      salary: Number(form.salary) || 0,
      branchId: form.branchId,
      departmentId: form.departmentId || null,
    };

    try {
      if (isEdit) {
        await api.put(`/api/employees/${employeeId}`, { id: employeeId, ...basePayload, status: form.status });
      } else {
        await api.post("/api/employees", {
          ...basePayload,
          hireDate: new Date(form.hireDate).toISOString(),
          applicationUserId: form.applicationUserId || null,
        });
      }
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("employees.failedToSave"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      title={isEdit ? t("employees.editEmployee") : t("employees.addEmployee")}
      subtitle={isEdit ? t("employees.updateRecord") : t("employees.addStaffMember")}
      onClose={onClose}
    >
      {isLoading ? (
        <p className="py-8 text-center font-mono text-sm text-ink-muted">{t("common.loading")}</p>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4">
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
              <label className={labelClass}>{t("employees.position")}</label>
              <input
                className={inputClass}
                value={form.position}
                onChange={(e) => set("position", e.target.value)}
                required
                maxLength={100}
              />
            </div>
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
              <label className={labelClass}>{t("employees.departmentOptional")}</label>
              <select
                className={inputClass}
                value={form.departmentId}
                onChange={(e) => set("departmentId", e.target.value)}
                disabled={!form.branchId}
              >
                <option value="">{t("employees.noDepartment")}</option>
                {departments.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.name}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className={labelClass}>{t("employees.salary")}</label>
              <input
                type="number"
                min={0}
                step="0.01"
                className={inputClass}
                value={form.salary}
                onChange={(e) => set("salary", e.target.value)}
                required
              />
            </div>
            {isEdit ? (
              <div>
                <label className={labelClass}>{t("common.status")}</label>
                <select className={inputClass} value={form.status} onChange={(e) => set("status", e.target.value)}>
                  {Object.keys(EMPLOYEE_STATUS_STYLES).map((value) => (
                    <option key={value} value={value}>
                      {translateEmployeeStatus(value, t)}
                    </option>
                  ))}
                </select>
              </div>
            ) : (
              <div>
                <label className={labelClass}>{t("employees.hireDate")}</label>
                <input
                  type="date"
                  className={inputClass}
                  value={form.hireDate}
                  onChange={(e) => set("hireDate", e.target.value)}
                  required
                />
              </div>
            )}
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
          </div>

          <div className="flex justify-end gap-2 border-t border-line pt-5">
            <button type="button" className={secondaryButtonClass} onClick={onClose} disabled={isSaving}>
              {t("common.cancel")}
            </button>
            <button type="submit" className={primaryButtonClass} disabled={isSaving}>
              {isSaving ? t("common.saving") : isEdit ? t("fields.saveChanges") : t("employees.addEmployee")}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
