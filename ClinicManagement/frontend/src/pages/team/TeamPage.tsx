import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { useAuth } from "../../lib/auth";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass } from "../../lib/ui";

const ASSIGNABLE_ROLES = ["Doctor", "Receptionist", "Accountant", "Nurse", "ClinicOwner"];

function generatePassword(): string {
  const upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
  const lower = "abcdefghijkmnpqrstuvwxyz";
  const digits = "23456789";
  const special = "!@#$%*?";
  const pick = (chars: string) => chars[Math.floor(Math.random() * chars.length)];
  const body = Array.from({ length: 8 }, () => pick(upper + lower + digits)).join("");
  return `${pick(upper)}${body}${pick(digits)}${pick(special)}`;
}

interface InvitedAccount {
  userId: string;
  fullName: string;
  email: string;
  role: string;
  password: string;
}

export function TeamPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [role, setRole] = useState("Doctor");
  const [password, setPassword] = useState(generatePassword());
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<string[]>([]);
  const [invited, setInvited] = useState<InvitedAccount[]>([]);
  const [copiedId, setCopiedId] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setFieldErrors([]);
    setIsSubmitting(true);
    try {
      const userId = await api.post<string>("/api/auth/register-user", { fullName, email, password, role });
      setInvited((prev) => [{ userId, fullName, email, role, password }, ...prev]);
      setFullName("");
      setEmail("");
      setRole("Doctor");
      setPassword(generatePassword());
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
        setFieldErrors(err.errors ?? []);
      } else {
        setError(t("team.failedToCreate"));
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  function copy(value: string, key: string) {
    navigator.clipboard.writeText(value).then(() => {
      setCopiedId(key);
      setTimeout(() => setCopiedId(null), 1500);
    });
  }

  return (
    <AppShell title={t("nav.team")} roleLabel={roles[0] ?? ""}>
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-[380px,1fr]">
        <div className="rounded-lg border border-line bg-surface p-5 shadow-card">
          <p className="font-medium text-ink">{t("team.inviteStaffMember")}</p>
          <p className="mt-1 text-sm text-ink-muted">
            {t("team.inviteExplainer")}
          </p>

          <form onSubmit={handleSubmit} className="mt-4 space-y-4">
            {(error || fieldErrors.length > 0) && (
              <div className="rounded-sm bg-danger-light px-3.5 py-2.5 text-sm text-danger">
                {error && <p>{error}</p>}
                {fieldErrors.length > 0 && (
                  <ul className="mt-1 list-inside list-disc">
                    {fieldErrors.map((msg, i) => (
                      <li key={i}>{msg}</li>
                    ))}
                  </ul>
                )}
              </div>
            )}

            <div>
              <label className={labelClass}>{t("fields.fullName")}</label>
              <input
                className={inputClass}
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                required
                maxLength={200}
              />
            </div>
            <div>
              <label className={labelClass}>{t("common.email")}</label>
              <input
                type="email"
                className={inputClass}
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <div>
              <label className={labelClass}>{t("team.role")}</label>
              <select className={inputClass} value={role} onChange={(e) => setRole(e.target.value)}>
                {ASSIGNABLE_ROLES.map((r) => (
                  <option key={r} value={r}>
                    {t(`enums.roleName.${r}`, r)}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className={labelClass}>{t("team.temporaryPassword")}</label>
              <div className="flex gap-2">
                <input
                  className={inputClass}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  minLength={8}
                />
                <button
                  type="button"
                  className="shrink-0 rounded-sm border border-line px-3 text-xs font-medium text-harbor-500 hover:bg-canvas"
                  onClick={() => setPassword(generatePassword())}
                >
                  {t("team.regenerate")}
                </button>
              </div>
              <p className="mt-1 text-xs text-ink-faint">{t("team.passwordHint")}</p>
            </div>

            <button type="submit" className={`${primaryButtonClass} w-full`} disabled={isSubmitting}>
              {isSubmitting ? t("team.creating") : t("team.createAccount")}
            </button>
          </form>
        </div>

        <div>
          <p className="mb-3 text-sm font-medium text-ink">{t("team.accountsCreated")}</p>
          {invited.length === 0 ? (
            <p className="rounded-lg border border-dashed border-line px-5 py-10 text-center text-sm text-ink-faint">
              {t("team.newAccountsExplainer")}
            </p>
          ) : (
            <div className="space-y-3">
              {invited.map((acc) => (
                <div key={acc.userId} className="rounded-lg border border-line bg-surface p-4 shadow-card">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <div>
                      <p className="font-medium text-ink">{acc.fullName}</p>
                      <p className="text-xs text-ink-faint">
                        {acc.email} · {acc.role}
                      </p>
                    </div>
                  </div>
                  <div className="mt-3 grid grid-cols-1 gap-2 sm:grid-cols-2">
                    <div className="flex items-center justify-between rounded-sm bg-canvas px-3 py-2">
                      <span className="truncate font-mono text-xs text-ink-muted">{acc.userId}</span>
                      <button
                        type="button"
                        className="ms-2 shrink-0 text-xs font-medium text-harbor-500 hover:underline"
                        onClick={() => copy(acc.userId, `${acc.userId}-id`)}
                      >
                        {copiedId === `${acc.userId}-id` ? t("team.copied") : t("team.copyId")}
                      </button>
                    </div>
                    <div className="flex items-center justify-between rounded-sm bg-canvas px-3 py-2">
                      <span className="truncate font-mono text-xs text-ink-muted">{acc.password}</span>
                      <button
                        type="button"
                        className="ms-2 shrink-0 text-xs font-medium text-harbor-500 hover:underline"
                        onClick={() => copy(acc.password, `${acc.userId}-pw`)}
                      >
                        {copiedId === `${acc.userId}-pw` ? t("team.copied") : t("team.copy")}
                      </button>
                    </div>
                  </div>
                  {(acc.role === "Doctor" || acc.role === "Nurse" || acc.role === "Receptionist" || acc.role === "Accountant") && (
                    <p className="mt-2 text-xs text-ink-faint">
                      {acc.role === "Doctor" ? t("team.pasteDoctorHint") : t("team.pasteEmployeeHint")}
                    </p>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </AppShell>
  );
}
