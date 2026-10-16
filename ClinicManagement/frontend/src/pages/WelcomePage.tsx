import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { AppShell, ALL_NAV_ITEMS } from "../components/AppShell";
import { useAuth } from "../lib/auth";

export function WelcomePage() {
  const { user } = useAuth();
  const { t } = useTranslation();
  const roles = user?.roles ?? [];

  const links = ALL_NAV_ITEMS.filter(
    (item) => item.to !== "overview" && (!item.roles || item.roles.some((r) => roles.includes(r))),
  );

  return (
    <AppShell title={t("welcome.title")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-6">
        <div>
          <h2 className="font-display text-2xl text-ink">{t("welcome.greeting", { name: user?.fullName?.split(" ")[0] })}</h2>
          <p className="mt-1.5 text-sm text-ink-muted">
            {links.length > 0
              ? t("welcome.hasAccess")
              : t("welcome.noAccess")}
          </p>
        </div>

        {links.length > 0 && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {links.map((item) => (
              <Link
                key={item.to}
                to={item.to}
                className="flex items-center gap-3 rounded-lg border border-line bg-surface p-5 shadow-card transition-colors hover:border-harbor-500"
              >
                <span className="text-xl leading-none text-harbor-500">{item.icon}</span>
                <span className="font-medium text-ink">{t(item.labelKey)}</span>
              </Link>
            ))}
          </div>
        )}
      </div>
    </AppShell>
  );
}
