interface StatCardProps {
  label: string;
  value: string | number;
  caption?: string;
  accent?: "harbor" | "gold" | "success" | "warning" | "danger";
}

const ACCENT_CLASSES: Record<NonNullable<StatCardProps["accent"]>, string> = {
  harbor: "text-harbor-500",
  gold: "text-gold-dark",
  success: "text-success",
  warning: "text-warning",
  danger: "text-danger",
};

export function StatCard({ label, value, caption, accent = "harbor" }: StatCardProps) {
  return (
    <div className="rounded-lg border border-line bg-surface p-5 shadow-card">
      <p className="text-xs font-medium uppercase tracking-[0.1em] text-ink-faint">{label}</p>
      <p className={`mt-2 font-display text-3xl font-medium tabular-nums ${ACCENT_CLASSES[accent]}`}>{value}</p>
      {caption && <p className="mt-1 text-xs text-ink-muted">{caption}</p>}
    </div>
  );
}
