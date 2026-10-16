interface ScheduleRulerProps {
  /** Which hour-slots (0-23 index within the visible range) are "occupied" — purely decorative unless a real occupancy array is passed in. */
  filled?: number[];
  hours?: number;
  className?: string;
  tone?: "light" | "dark";
}

/**
 * The product's one recurring signature element: a thin horizontal strip of
 * hour ticks, some filled (booked), most open — an abstracted appointment
 * day. Used large and ambient on the login screen, small and literal (with
 * real data) as a rhythm indicator on dashboard cards. Deliberately not a
 * medical cross or heartbeat line — grounded in scheduling, which is what
 * this product actually does all day.
 */
export function ScheduleRuler({ filled = [], hours = 24, className = "", tone = "light" }: ScheduleRulerProps) {
  const filledSet = new Set(filled);
  const dotColor = tone === "dark" ? "bg-gold" : "bg-harbor-500";
  const emptyColor = tone === "dark" ? "bg-white/15" : "bg-harbor-200";
  const lineColor = tone === "dark" ? "bg-white/10" : "bg-line";

  return (
    <div className={`relative flex items-center gap-[3px] ${className}`} aria-hidden="true">
      <div className={`absolute left-0 right-0 h-px ${lineColor}`} />
      {Array.from({ length: hours }).map((_, i) => (
        <div
          key={i}
          className={`relative z-10 rounded-full transition-all ${
            filledSet.has(i) ? `${dotColor} h-2 w-2` : `${emptyColor} h-1 w-1`
          }`}
        />
      ))}
    </div>
  );
}
