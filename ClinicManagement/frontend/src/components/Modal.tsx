import { useEffect, type ReactNode } from "react";
import { useTranslation } from "react-i18next";

interface ModalProps {
  title: string;
  subtitle?: string;
  onClose: () => void;
  children: ReactNode;
  width?: "md" | "lg" | "xl";
}

const WIDTH_CLASSES: Record<NonNullable<ModalProps["width"]>, string> = {
  md: "max-w-md",
  lg: "max-w-lg",
  xl: "max-w-2xl",
};

export function Modal({ title, subtitle, onClose, children, width = "lg" }: ModalProps) {
  const { t } = useTranslation();
  useEffect(() => {
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") onClose();
    }
    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, [onClose]);

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-ink/40 px-4 py-10 backdrop-blur-[2px]">
      <div
        className={`w-full ${WIDTH_CLASSES[width]} rounded-lg border border-line bg-surface shadow-card`}
        role="dialog"
        aria-modal="true"
      >
        <div className="flex items-start justify-between border-b border-line px-6 py-5">
          <div>
            <h2 className="font-display text-xl text-ink">{title}</h2>
            {subtitle && <p className="mt-0.5 text-sm text-ink-muted">{subtitle}</p>}
          </div>
          <button
            onClick={onClose}
            aria-label={t("common.close")}
            className="rounded-sm p-1 text-ink-faint transition-colors hover:bg-canvas hover:text-ink"
          >
            ✕
          </button>
        </div>
        <div className="px-6 py-5">{children}</div>
      </div>
    </div>
  );
}
