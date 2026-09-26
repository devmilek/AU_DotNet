export type DateKey = string;

const DAY_MS = 86_400_000;

const dateKeyFormatters = new Map<string, Intl.DateTimeFormat>();

function dateKeyFormatter(timeZone: string) {
  let formatter = dateKeyFormatters.get(timeZone);
  if (!formatter) {
    formatter = new Intl.DateTimeFormat("en-CA", {
      timeZone,
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
    });
    dateKeyFormatters.set(timeZone, formatter);
  }
  return formatter;
}

export function toWallClock(date: DateKey, time: string): Date {
  const [year, month, day] = date.split("-").map(Number);
  const [hours, minutes] = time.split(":").map(Number);
  return new Date(Date.UTC(year, month - 1, day, hours, minutes));
}

export function parseLocalDateTime(value: string): Date {
  return new Date(`${value.slice(0, 19)}Z`);
}

export function toLocalDateTimeString(wallClock: Date): string {
  return wallClock.toISOString().slice(0, 19);
}

export function addMinutes(date: Date, minutes: number): Date {
  return new Date(date.getTime() + minutes * 60_000);
}

export function dateKeyOf(instant: Date, timeZone: string): DateKey {
  return dateKeyFormatter(timeZone).format(instant);
}

export function addDays(key: DateKey, days: number): DateKey {
  const [year, month, day] = key.split("-").map(Number);
  return new Date(Date.UTC(year, month - 1, day) + days * DAY_MS)
    .toISOString()
    .slice(0, 10);
}

export function localDateKey(date: Date): DateKey {
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${date.getFullYear()}-${month}-${day}`;
}

export function fromLocalDateKey(key: DateKey): Date {
  const [year, month, day] = key.split("-").map(Number);
  return new Date(year, month - 1, day);
}

export function wallClockNow(timeZone: string, now = new Date()): Date {
  const parts = new Intl.DateTimeFormat("en-CA", {
    timeZone,
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    hourCycle: "h23",
  }).formatToParts(now);
  const part = (type: Intl.DateTimeFormatPartTypes) =>
    Number(parts.find((p) => p.type === type)?.value);

  return new Date(
    Date.UTC(part("year"), part("month") - 1, part("day"), part("hour"), part("minute")),
  );
}
