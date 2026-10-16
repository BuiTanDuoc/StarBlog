import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth, primaryRouteForRoles, ApiError } from "../lib/auth";
import { ScheduleRuler } from "../components/ScheduleRuler";
import { LanguageSwitcher } from "../components/LanguageSwitcher";

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      const user = await login(email, password);
      navigate(primaryRouteForRoles(user.roles));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("common.somethingWentWrong"));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="flex min-h-screen">
      {/* Editorial panel */}
      <div className="relative hidden w-1/2 flex-col justify-between overflow-hidden bg-harbor-700 px-14 py-12 text-white lg:flex">
        <div className="absolute inset-0 opacity-[0.07]" aria-hidden="true">
          <div className="absolute -right-24 -top-24 h-[420px] w-[420px] rounded-full border border-white" />
          <div className="absolute -bottom-32 -left-16 h-[320px] w-[320px] rounded-full border border-white" />
        </div>

        <p className="relative font-display text-xl italic">{t("login.brand")}</p>

        <div className="relative max-w-md">
          <p className="font-display text-[2.75rem] italic leading-[1.15] tracking-tight">
            {t("login.headline")}
          </p>
          <p className="mt-5 text-harbor-100">
            {t("login.subheadline")}
          </p>

          <ScheduleRuler
            tone="dark"
            hours={32}
            filled={[2, 3, 4, 8, 9, 13, 14, 15, 16, 21, 22, 27]}
            className="mt-10"
          />
          <p className="mt-3 font-mono text-[11px] uppercase tracking-[0.14em] text-harbor-200">
            {t("login.todayAcrossBranches")}
          </p>
        </div>

        <p className="relative font-mono text-xs text-harbor-200">© {new Date().getFullYear()} {t("login.brand")}</p>
      </div>

      {/* Form panel */}
      <div className="flex w-full flex-col items-center justify-center bg-canvas px-6 lg:w-1/2">
        <div className="w-full max-w-sm">
          <div className="flex items-center justify-between lg:justify-end">
            <p className="font-display text-2xl italic text-ink lg:hidden">{t("login.brand")}</p>
            <LanguageSwitcher />
          </div>
          <h1 className="mt-6 font-display text-3xl text-ink">{t("login.signIn")}</h1>
          <p className="mt-1.5 text-sm text-ink-muted">{t("login.prompt")}</p>

          <form onSubmit={handleSubmit} className="mt-8 space-y-4" noValidate>
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-ink">
                {t("common.email")}
              </label>
              <input
                id="email"
                type="email"
                autoComplete="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="mt-1.5 w-full rounded-sm border border-line bg-surface px-3.5 py-2.5 text-sm text-ink outline-none transition-colors focus:border-harbor-500"
                placeholder="you@clinic.com"
              />
            </div>

            <div>
              <label htmlFor="password" className="block text-sm font-medium text-ink">
                {t("login.password")}
              </label>
              <input
                id="password"
                type="password"
                autoComplete="current-password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="mt-1.5 w-full rounded-sm border border-line bg-surface px-3.5 py-2.5 text-sm text-ink outline-none transition-colors focus:border-harbor-500"
                placeholder="••••••••"
              />
            </div>

            {error && (
              <p role="alert" className="rounded-sm bg-danger-light px-3.5 py-2.5 text-sm text-danger">
                {error}
              </p>
            )}

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full rounded-sm bg-harbor-500 py-2.5 text-sm font-medium text-white transition-colors hover:bg-harbor-600 disabled:opacity-60"
            >
              {isSubmitting ? t("login.signingIn") : t("login.signIn")}
            </button>
          </form>

          <p className="mt-8 text-center text-xs text-ink-faint">
            {t("login.newClinic")}{" "}
            <a href="/register" className="font-medium text-harbor-500 underline-offset-2 hover:underline">
              {t("login.setupWorkspace")}
            </a>
          </p>
        </div>
      </div>
    </div>
  );
}
