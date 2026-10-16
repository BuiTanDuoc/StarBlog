import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth, primaryRouteForRoles, ApiError } from "../lib/auth";
import { ScheduleRuler } from "../components/ScheduleRuler";
import { LanguageSwitcher } from "../components/LanguageSwitcher";

function slugify(value: string): string {
  return value
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "");
}

const inputClass =
  "mt-1.5 w-full rounded-sm border border-line bg-surface px-3.5 py-2.5 text-sm text-ink outline-none transition-colors focus:border-harbor-500";
const labelClass = "block text-sm font-medium text-ink";

export function RegisterPage() {
  const { registerTenant } = useAuth();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [clinicName, setClinicName] = useState("");
  const [slug, setSlug] = useState("");
  const [slugEdited, setSlugEdited] = useState(false);
  const [ownerFullName, setOwnerFullName] = useState("");
  const [ownerEmail, setOwnerEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function handleClinicNameChange(value: string) {
    setClinicName(value);
    if (!slugEdited) setSlug(slugify(value));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setFieldErrors([]);
    setIsSubmitting(true);
    try {
      const user = await registerTenant({
        clinicName,
        slug,
        ownerFullName,
        ownerEmail,
        password,
      });
      navigate(primaryRouteForRoles(user.roles));
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
        setFieldErrors(err.errors ?? []);
      } else {
        setError(t("common.somethingWentWrong"));
      }
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
            {t("register.headline")}
          </p>
          <p className="mt-5 text-harbor-100">
            {t("register.subheadline")}
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
      <div className="flex w-full flex-col items-center justify-center bg-canvas px-6 py-10 lg:w-1/2">
        <div className="w-full max-w-sm">
          <div className="flex items-center justify-between lg:justify-end">
            <p className="font-display text-2xl italic text-ink lg:hidden">{t("login.brand")}</p>
            <LanguageSwitcher />
          </div>
          <h1 className="mt-6 font-display text-3xl text-ink">{t("register.setupWorkspace")}</h1>
          <p className="mt-1.5 text-sm text-ink-muted">{t("register.prompt")}</p>

          <form onSubmit={handleSubmit} className="mt-8 space-y-4" noValidate>
            <div>
              <label htmlFor="clinicName" className={labelClass}>
                {t("register.clinicName")}
              </label>
              <input
                id="clinicName"
                required
                value={clinicName}
                onChange={(e) => handleClinicNameChange(e.target.value)}
                className={inputClass}
                placeholder="Riverside Family Clinic"
                maxLength={200}
              />
            </div>

            <div>
              <label htmlFor="slug" className={labelClass}>
                {t("register.workspaceUrl")}
              </label>
              <input
                id="slug"
                required
                value={slug}
                onChange={(e) => {
                  setSlugEdited(true);
                  setSlug(slugify(e.target.value));
                }}
                className={inputClass}
                placeholder="riverside-family-clinic"
                pattern="[a-z0-9-]+"
                maxLength={100}
              />
              <p className="mt-1 text-xs text-ink-faint">{t("register.slugHint")}</p>
            </div>

            <div>
              <label htmlFor="ownerFullName" className={labelClass}>
                {t("register.yourFullName")}
              </label>
              <input
                id="ownerFullName"
                autoComplete="name"
                required
                value={ownerFullName}
                onChange={(e) => setOwnerFullName(e.target.value)}
                className={inputClass}
                placeholder="Dr. Amina Hassan"
                maxLength={200}
              />
            </div>

            <div>
              <label htmlFor="ownerEmail" className={labelClass}>
                {t("common.email")}
              </label>
              <input
                id="ownerEmail"
                type="email"
                autoComplete="email"
                required
                value={ownerEmail}
                onChange={(e) => setOwnerEmail(e.target.value)}
                className={inputClass}
                placeholder="you@clinic.com"
              />
            </div>

            <div>
              <label htmlFor="password" className={labelClass}>
                {t("login.password")}
              </label>
              <input
                id="password"
                type="password"
                autoComplete="new-password"
                required
                minLength={8}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className={inputClass}
                placeholder="••••••••"
              />
              <p className="mt-1 text-xs text-ink-faint">
                {t("register.passwordHint")}
              </p>
            </div>

            {(error || fieldErrors.length > 0) && (
              <div role="alert" className="rounded-sm bg-danger-light px-3.5 py-2.5 text-sm text-danger">
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

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full rounded-sm bg-harbor-500 py-2.5 text-sm font-medium text-white transition-colors hover:bg-harbor-600 disabled:opacity-60"
            >
              {isSubmitting ? t("register.settingUp") : t("register.createWorkspace")}
            </button>
          </form>

          <p className="mt-8 text-center text-xs text-ink-faint">
            {t("register.alreadyHaveWorkspace")}{" "}
            <a href="/login" className="font-medium text-harbor-500 underline-offset-2 hover:underline">
              {t("login.signIn")}
            </a>
          </p>
        </div>
      </div>
    </div>
  );
}
