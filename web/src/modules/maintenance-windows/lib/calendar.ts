import { addDays, type DateKey } from "./wall-clock";

export type CalendarMonth = { year: number; month: number };

const GRID_DAYS = 42;

export function monthOf(key: DateKey): CalendarMonth {
  const [year, month] = key.split("-").map(Number);
  return { year, month: month - 1 };
}

export function shiftMonth(
  { year, month }: CalendarMonth,
  delta: number,
): CalendarMonth {
  const date = new Date(Date.UTC(year, month + delta, 1));
  return { year: date.getUTCFullYear(), month: date.getUTCMonth() };
}

export function isSameMonth(a: CalendarMonth, b: CalendarMonth): boolean {
  return a.year === b.year && a.month === b.month;
}

export function monthGrid({ year, month }: CalendarMonth): DateKey[] {
  const first = new Date(Date.UTC(year, month, 1));
  const mondayOffset = (first.getUTCDay() + 6) % 7;
  const start = addDays(first.toISOString().slice(0, 10), -mondayOffset);
  return Array.from({ length: GRID_DAYS }, (_, index) => addDays(start, index));
}

export function isInMonth(key: DateKey, { year, month }: CalendarMonth) {
  const [keyYear, keyMonth] = key.split("-").map(Number);
  return keyYear === year && keyMonth - 1 === month;
}

export function formatMonthTitle({ year, month }: CalendarMonth): string {
  return new Intl.DateTimeFormat(undefined, {
    month: "long",
    year: "numeric",
    timeZone: "UTC",
  }).format(new Date(Date.UTC(year, month, 1)));
}

export function weekdayHeaders(): { short: string; long: string }[] {
  const monday = Date.UTC(2024, 0, 1);
  return Array.from({ length: 7 }, (_, index) => {
    const date = new Date(monday + index * 86_400_000);
    return {
      short: new Intl.DateTimeFormat(undefined, {
        weekday: "short",
        timeZone: "UTC",
      }).format(date),
      long: new Intl.DateTimeFormat(undefined, {
        weekday: "long",
        timeZone: "UTC",
      }).format(date),
    };
  });
}
