import type { ReactNode } from "react";
import { NavLink } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { primaryRouteForRoles, useAuth } from "../lib/auth";
import { NotificationBell } from "./NotificationBell";
import { LanguageSwitcher } from "./LanguageSwitcher";

interface AppShellProps {
  children: ReactNode;
  title: string;
  roleLabel: string;
}

export interface NavItem {
  labelKey: string;
  icon: string;
  to: string;
  roles?: string[];
}

const CLINICAL_STAFF_ROLES = ["ClinicOwner", "Doctor", "Receptionist", "Nurse", "SuperAdmin"];

export const ALL_NAV_ITEMS: NavItem[] = [
  { labelKey: "nav.overview", icon: "◱", to: "overview" },
  { labelKey: "nav.patients", icon: "◉", to: "/patients", roles: CLINICAL_STAFF_ROLES },
  { labelKey: "nav.appointments", icon: "▤", to: "/appointments", roles: CLINICAL_STAFF_ROLES },
  { labelKey: "nav.doctors", icon: "✚", to: "/doctors" },
  { labelKey: "nav.branches", icon: "◈", to: "/branches", roles: CLINICAL_STAFF_ROLES },
  { labelKey: "nav.departments", icon: "▥", to: "/departments", roles: CLINICAL_STAFF_ROLES },
  { labelKey: "nav.employees", icon: "◍", to: "/employees", roles: ["ClinicOwner", "SuperAdmin"] },
  { labelKey: "nav.team", icon: "◫", to: "/team", roles: ["ClinicOwner"] },
  { labelKey: "nav.invoices", icon: "▧", to: "/invoices", roles: ["ClinicOwner", "Receptionist", "Accountant", "SuperAdmin"] },
  { labelKey: "nav.expenses", icon: "◭", to: "/expenses", roles: ["ClinicOwner", "Accountant", "SuperAdmin"] },
  { labelKey: "nav.reports", icon: "▨", to: "/reports", roles: ["ClinicOwner", "Doctor", "Accountant", "SuperAdmin"] },
  { labelKey: "nav.examTemplates", icon: "▧", to: "/medical-record-templates", roles: ["ClinicOwner", "SuperAdmin"] },
  { labelKey: "nav.treatmentPackages", icon: "◈", to: "/treatment-packages", roles: ["ClinicOwner", "SuperAdmin"] },
  { labelKey: "nav.subscription", icon: "◐", to: "/subscription", roles: ["ClinicOwner", "SuperAdmin"] },
  { labelKey: "nav.auditLog", icon: "▦", to: "/audit-logs", roles: ["ClinicOwner", "SuperAdmin"] },
  { labelKey: "nav.plans", icon: "◆", to: "/plans", roles: ["SuperAdmin"] },
];

export function AppShell({ children, title, roleLabel }: AppShellProps) {
  const { user, logout } = useAuth();
  const { t, i18n } = useTranslation();
  const overviewRoute = user ? primaryRouteForRoles(user.roles) : "/welcome";
  const navItems = ALL_NAV_ITEMS.filter((item) => !item.roles || item.roles.some((r) => user?.roles.includes(r)));
  const dateLocale = i18n.resolvedLanguage === "ar" ? "ar-EG" : "en-US";

  return (
    <div className="flex min-h-screen bg-canvas">
      <aside className="flex w-60 flex-col border-e border-line bg-harbor-700 text-white">
        <div className="px-6 py-7">
          <p className="font-display text-xl italic tracking-tight">{t("common.appName")}</p>
          <p className="mt-0.5 text-xs uppercase tracking-[0.14em] text-harbor-200">{t("common.appNameSub")}</p>
        </div>

        <nav className="flex-1 px-3">
          {navItems.map((item) => (
            <NavLink
              key={item.labelKey}
              to={item.to === "overview" ? overviewRoute : item.to}
              end={item.to === "overview"}
              className={({ isActive }) =>
                `mb-1 flex w-full items-center gap-3 rounded-sm px-3 py-2.5 text-start text-sm transition-colors ${
                  isActive ? "bg-white/10 text-white" : "text-harbor-100 hover:bg-white/5 hover:text-white"
                }`
              }
            >
              <span className="text-base leading-none opacity-80">{item.icon}</span>
              {t(item.labelKey)}
            </NavLink>
          ))}
        </nav>

        <div className="border-t border-white/10 px-4 py-4">
          <p className="truncate text-sm font-medium">{user?.fullName}</p>
          <p className="truncate text-xs text-harbor-200">{roleLabel}</p>
          <button
            onClick={logout}
            className="mt-3 text-xs font-medium text-harbor-200 underline decoration-white/30 underline-offset-2 hover:text-white"
          >
            {t("nav.signOut")}
          </button>
        </div>
      </aside>

      <div className="flex flex-1 flex-col">
        <header className="flex items-center justify-between border-b border-line bg-surface px-8 py-5">
          <h1 className="font-display text-2xl text-ink">{title}</h1>
          <div className="flex items-center gap-4">
            <span className="font-mono text-xs text-ink-faint">
              {new Date().toLocaleDateString(dateLocale, { weekday: "long", month: "long", day: "numeric" })}
            </span>
            <LanguageSwitcher compact />
            <NotificationBell />
            <NavLink
              to="/account"
              className="flex h-9 w-9 items-center justify-center rounded-full bg-harbor-100 font-display text-sm text-harbor-700 hover:bg-harbor-200"
              title={t("nav.account")}
            >
              {user?.fullName?.charAt(0) ?? "?"}
            </NavLink>
          </div>
        </header>

        <main className="flex-1 px-8 py-7">{children}</main>
      </div>
    </div>
  );
}
