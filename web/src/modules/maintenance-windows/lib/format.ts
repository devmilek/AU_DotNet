export function formatDuration(totalMinutes: number): string {
  const days = Math.floor(totalMinutes / 1440);
  const hours = Math.floor((totalMinutes % 1440) / 60);
  const minutes = totalMinutes % 60;

  return (
    [
      days > 0 ? `${days} d` : null,
      hours > 0 ? `${hours} h` : null,
      minutes > 0 ? `${minutes} min` : null,
    ]
      .filter(Boolean)
      .join(" ") || "0 min"
  );
}

export function formatTime(date: Date, timeZone: string): string {
  return new Intl.DateTimeFormat(undefined, {
    hour: "2-digit",
    minute: "2-digit",
    hourCycle: "h23",
    timeZone,
  }).format(date);
}

export function formatDate(
  date: Date,
  timeZone: string,
  options: Intl.DateTimeFormatOptions = {
    weekday: "short",
    day: "numeric",
    month: "short",
    year: "numeric",
  },
): string {
  return new Intl.DateTimeFormat(undefined, { ...options, timeZone }).format(
    date,
  );
}

export function formatTimeRange(start: Date, end: Date, timeZone: string) {
  return `${formatTime(start, timeZone)}–${formatTime(end, timeZone)}`;
}

const relativeFormatter = new Intl.RelativeTimeFormat(undefined, {
  numeric: "auto",
});

export function formatRelative(target: Date, now: Date): string {
  const diffMinutes = Math.round((target.getTime() - now.getTime()) / 60_000);
  const absMinutes = Math.abs(diffMinutes);

  if (absMinutes < 60) return relativeFormatter.format(diffMinutes, "minute");
  if (absMinutes < 1440) {
    return relativeFormatter.format(Math.round(diffMinutes / 60), "hour");
  }
  return relativeFormatter.format(Math.round(diffMinutes / 1440), "day");
}
