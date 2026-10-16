import type { ReactNode } from "react";
import { NavLink } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../lib/auth";
import { LanguageSwitcher } from "../../components/LanguageSwitcher";

const NAV_ITEMS = [
  { labelKey: "portal.navOverview", to: "/portal" },
  { labelKey: "portal.navBook", to: "/portal/book" },
  { labelKey: "portal.navAppointments", to: "/portal/appointments" },
  { labelKey: "portal.navMedicalRecords", to: "/portal/medical-records" },
  { labelKey: "portal.navInvoices", to: "/portal/invoices" },
  { labelKey: "portal.navProfile", to: "/portal/profile" },
];

/**
 * Intentionally a separate, simpler shell from the staff AppShell (top nav,
 * no sidebar) — a patient isn't "staff" and shouldn't see a UI built around
 * managing a whole clinic.
 */
export function PortalShell({ children, title }: { children: ReactNode; title: string }) {
  const { user, logout } = useAuth();
  const { t } = useTranslation();

  return (
    <div className="min-h-screen bg-canvas">
      <header className="border-b border-line bg-surface">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-6 py-4">
          <div>
            <p className="font-display text-lg italic text-ink">{t("portal.patientPortal")}</p>
            {user && <p className="text-xs text-ink-faint">{user.fullName}</p>}
          </div>
          <div className="flex items-center gap-4">
            <LanguageSwitcher compact />
            <button onClick={logout} className="text-sm font-medium text-harbor-500 hover:underline">
              {t("nav.signOut")}
            </button>
          </div>
        </div>
        <nav className="mx-auto flex max-w-5xl gap-1 px-6">
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === "/portal"}
              className={({ isActive }) =>
                `border-b-2 px-3 py-2.5 text-sm transition-colors ${
                  isActive
                    ? "border-harbor-500 font-medium text-harbor-500"
                    : "border-transparent text-ink-muted hover:text-ink"
                }`
              }
            >
              {t(item.labelKey)}
            </NavLink>
          ))}
        </nav>
      </header>

      <main className="mx-auto max-w-5xl px-6 py-8">
        <h1 className="font-display text-2xl text-ink">{title}</h1>
        <div className="mt-6">{children}</div>
      </main>
    </div>
  );
}
