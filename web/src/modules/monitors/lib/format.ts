const durationUnits = [
  { unit: "year", ms: 365 * 24 * 60 * 60 * 1000 },
  { unit: "month", ms: 30 * 24 * 60 * 60 * 1000 },
  { unit: "day", ms: 24 * 60 * 60 * 1000 },
  { unit: "hour", ms: 60 * 60 * 1000 },
  { unit: "minute", ms: 60 * 1000 },
  { unit: "second", ms: 1000 },
] as const;

/** "7 days", "3 hours", "less than a minute" — tylko największa jednostka. */
export function formatDuration(from: Date, to: Date): string {
  const elapsed = Math.max(0, to.getTime() - from.getTime());
  if (elapsed < 60 * 1000) return "less than a minute";

  const { unit, ms } = durationUnits.find((u) => elapsed >= u.ms)!;
  return new Intl.NumberFormat(undefined, {
    style: "unit",
    unit,
    unitDisplay: "long",
  }).format(Math.floor(elapsed / ms));
}

const relativeTimeFormatter = new Intl.RelativeTimeFormat(undefined, {
  numeric: "auto",
});

/** "12 seconds ago", "3 minutes ago"; poniżej 5 s — "just now". */
export function formatRelativeTime(date: Date, now: Date): string {
  const elapsed = now.getTime() - date.getTime();
  if (elapsed < 5000) return "just now";

  const { unit, ms } = durationUnits.find((u) => elapsed >= u.ms)!;
  return relativeTimeFormatter.format(-Math.floor(elapsed / ms), unit);
}

/** "30 seconds", "5 minutes", "1 hour". */
export function formatInterval(seconds: number): string {
  const unit =
    seconds % 3600 === 0 ? "hour" : seconds % 60 === 0 ? "minute" : "second";
  const value =
    unit === "hour"
      ? seconds / 3600
      : unit === "minute"
        ? seconds / 60
        : seconds;

  return new Intl.NumberFormat(undefined, {
    style: "unit",
    unit,
    unitDisplay: "long",
  }).format(value);
}

const millisecondsFormatter = new Intl.NumberFormat(undefined, {
  maximumFractionDigits: 0,
});

export function formatMilliseconds(value: number | null | undefined): string {
  return value == null ? "—" : `${millisecondsFormatter.format(value)} ms`;
}

const uptimeFormatter = new Intl.NumberFormat(undefined, {
  style: "percent",
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

/** Uptime z dokładnością do 0,01 pp; null (brak checków) — "—". */
export function formatUptime(ratio: number | null | undefined): string {
  return ratio == null ? "—" : uptimeFormatter.format(ratio);
}
