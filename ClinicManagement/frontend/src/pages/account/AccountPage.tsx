import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { useAuth } from "../../lib/auth";
import { api, ApiError } from "../../lib/api";
import { inputClass, labelClass, primaryButtonClass } from "../../lib/ui";

export function AccountPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<string[]>([]);
  const [success, setSuccess] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setFieldErrors([]);
    setSuccess(false);

    if (newPassword !== confirmPassword) {
      setError(t("account.passwordMismatch"));
      return;
    }

    setIsSubmitting(true);
    try {
      await api.post("/api/auth/change-password", { currentPassword, newPassword });
      setSuccess(true);
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
        setFieldErrors(err.errors ?? []);
      } else {
        setError(t("account.failedToChangePassword"));
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AppShell title={t("nav.account")} roleLabel={roles[0] ?? ""}>
      <div className="max-w-md space-y-6">
        <div className="rounded-lg border border-line bg-surface p-5 shadow-card">
          <p className="text-xs uppercase tracking-[0.06em] text-ink-faint">{t("account.signedInAs")}</p>
          <p className="mt-1 font-medium text-ink">{user?.fullName}</p>
          <p className="text-sm text-ink-muted">{user?.email}</p>
          <p className="mt-1 text-xs text-ink-faint">{roles.map((r) => t(`enums.roleName.${r}`, r)).join(", ")}</p>
        </div>

        <div className="rounded-lg border border-line bg-surface p-5 shadow-card">
          <p className="font-medium text-ink">{t("account.changePassword")}</p>
          <p className="mt-1 text-sm text-ink-muted">{t("account.passwordReuseHint")}</p>

          <form onSubmit={handleSubmit} className="mt-4 space-y-4">
            {success && (
              <p className="rounded-sm bg-success-light px-3.5 py-2.5 text-sm text-success">
                {t("account.passwordChangedSuccess")}
              </p>
            )}
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
              <label className={labelClass}>{t("account.currentPassword")}</label>
              <input
                type="password"
                autoComplete="current-password"
                className={inputClass}
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
                required
              />
            </div>
            <div>
              <label className={labelClass}>{t("account.newPassword")}</label>
              <input
                type="password"
                autoComplete="new-password"
                className={inputClass}
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
                minLength={8}
              />
              <p className="mt-1 text-xs text-ink-faint">
                {t("register.passwordHint")}
              </p>
            </div>
            <div>
              <label className={labelClass}>{t("account.confirmNewPassword")}</label>
              <input
                type="password"
                autoComplete="new-password"
                className={inputClass}
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
                minLength={8}
              />
            </div>

            <button type="submit" className={primaryButtonClass} disabled={isSubmitting}>
              {isSubmitting ? t("common.saving") : t("account.changePassword")}
            </button>
          </form>
        </div>
      </div>
    </AppShell>
  );
}
