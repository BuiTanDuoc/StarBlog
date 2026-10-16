import { useTranslation } from "react-i18next";

export function LanguageSwitcher({ compact = false }: { compact?: boolean }) {
  const { i18n, t } = useTranslation();
  const current = i18n.resolvedLanguage || i18n.language || "en";

  function toggle() {
    i18n.changeLanguage(current === "ar" ? "en" : "ar");
  }

  if (compact) {
    return (
      <button
        onClick={toggle}
        title={t("language.label")}
        className="flex h-9 min-w-9 items-center justify-center rounded-full border border-line bg-surface px-2 text-xs font-medium text-ink-muted hover:border-harbor-200 hover:text-ink"
      >
        {current === "ar" ? "EN" : "AR"}
      </button>
    );
  }

  return (
    <div className="inline-flex items-center rounded-sm border border-line bg-surface p-0.5 text-xs font-medium">
      <button
        onClick={() => i18n.changeLanguage("en")}
        className={`rounded-sm px-2.5 py-1 transition-colors ${
          current === "en" ? "bg-harbor-700 text-white" : "text-ink-muted hover:text-ink"
        }`}
      >
        {t("language.english")}
      </button>
      <button
        onClick={() => i18n.changeLanguage("ar")}
        className={`rounded-sm px-2.5 py-1 transition-colors ${
          current === "ar" ? "bg-harbor-700 text-white" : "text-ink-muted hover:text-ink"
        }`}
      >
        {t("language.arabic")}
      </button>
    </div>
  );
}
