import { useState, type FormEvent } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth, primaryRouteForRoles, ApiError } from "../../lib/auth";
import { inputClass, labelClass, primaryButtonClass } from "../../lib/ui";
import { LanguageSwitcher } from "../../components/LanguageSwitcher";

/**
 * Public signup for a specific clinic, reached via "/portal/register/:slug"
 * (e.g. a link the clinic shares — "book with us at clinic.app/portal/register/riverside-clinic").
 * Always creates a brand-new patient record — see the comment on
 * RegisterPatientAccountCommand for why this never tries to auto-link an
 * existing reception-created patient by phone number.
 */
export function PortalRegisterPage() {
  const { t } = useTranslation();
  const { slug } = useParams<{ slug: string }>();
  const { registerPatient } = useAuth();
  const navigate = useNavigate();

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [password, setPassword] = useState("");
  const [dateOfBirth, setDateOfBirth] = useState("");
  const [gender, setGender] = useState<"Male" | "Female" | "Other">("Male");
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setFieldErrors([]);
    setIsSubmitting(true);
    try {
      const user = await registerPatient({
        tenantSlug: slug ?? "",
        fullName,
        email,
        phone,
        password,
        dateOfBirth,
        gender,
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
    <div className="flex min-h-screen items-center justify-center bg-canvas px-6 py-10">
      <div className="w-full max-w-sm">
        <div className="flex items-center justify-between">
          <p className="font-display text-2xl italic text-ink">{t("portal.patientPortal")}</p>
          <LanguageSwitcher />
        </div>
        <h1 className="mt-6 font-display text-3xl text-ink">{t("portal.createYourAccount")}</h1>
        <p className="mt-1.5 text-sm text-ink-muted">
          {t("portal.bookWithClinic")}{" "}
          <span className="font-medium text-ink">{slug ?? t("portal.yourClinic")}</span>.
        </p>

        <form onSubmit={handleSubmit} className="mt-8 space-y-4" noValidate>
          <div>
            <label htmlFor="fullName" className={labelClass}>{t("fields.fullName")}</label>
            <input
              id="fullName" required value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              className={inputClass} placeholder="Jane Doe" maxLength={200}
            />
          </div>

          <div>
            <label htmlFor="email" className={labelClass}>{t("common.email")}</label>
            <input
              id="email" type="email" autoComplete="email" required value={email}
              onChange={(e) => setEmail(e.target.value)}
              className={inputClass} placeholder="you@example.com"
            />
          </div>

          <div>
            <label htmlFor="phone" className={labelClass}>{t("common.phone")}</label>
            <input
              id="phone" required value={phone}
              onChange={(e) => setPhone(e.target.value)}
              className={inputClass} placeholder="+20 100 000 0000"
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label htmlFor="dateOfBirth" className={labelClass}>{t("fields.dateOfBirth")}</label>
              <input
                id="dateOfBirth" type="date" required value={dateOfBirth}
                onChange={(e) => setDateOfBirth(e.target.value)}
                className={inputClass}
              />
            </div>
            <div>
              <label htmlFor="gender" className={labelClass}>Gender</label>
              <select
                id="gender" value={gender}
                onChange={(e) => setGender(e.target.value as typeof gender)}
                className={inputClass}
              >
                <option value="Male">Male</option>
                <option value="Female">Female</option>
                <option value="Other">Other</option>
              </select>
            </div>
          </div>

          <div>
            <label htmlFor="password" className={labelClass}>{t("login.password")}</label>
            <input
              id="password" type="password" autoComplete="new-password" required minLength={8}
              value={password} onChange={(e) => setPassword(e.target.value)}
              className={inputClass} placeholder="••••••••"
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

          <button type="submit" disabled={isSubmitting} className={`${primaryButtonClass} w-full`}>
            {isSubmitting ? t("portal.creatingAccount") : t("team.createAccount")}
          </button>
        </form>

        <p className="mt-8 text-center text-xs text-ink-faint">
          {t("portal.alreadyHaveAccount")}{" "}
          <Link to="/login" className="font-medium text-harbor-500 underline-offset-2 hover:underline">
            {t("login.signIn")}
          </Link>
        </p>
      </div>
    </div>
  );
}
