const relativeFormatter = new Intl.RelativeTimeFormat(undefined, {
  numeric: "auto",
});

export function formatRelativeDays(target: Date, now: Date): string {
  const days = Math.round((target.getTime() - now.getTime()) / 86_400_000);
  if (Math.abs(days) >= 1) return relativeFormatter.format(days, "day");

  const hours = Math.round((target.getTime() - now.getTime()) / 3_600_000);
  return relativeFormatter.format(hours, "hour");
}

export function formatShortDate(date: Date): string {
  return new Intl.DateTimeFormat(undefined, {
    day: "numeric",
    month: "short",
    year: "numeric",
  }).format(date);
}

export function initialsOf(name: string): string {
  const parts = name.trim().split(/[\s@._-]+/).filter(Boolean);
  return (parts[0]?.[0] ?? "?").toUpperCase() + (parts[1]?.[0] ?? "").toUpperCase();
}
