import { RRule } from "rrule";
import { addMinutes, type DateKey, toWallClock } from "./wall-clock";

export const WEEKDAY_CODES = ["MO", "TU", "WE", "TH", "FR", "SA", "SU"] as const;
export type WeekdayCode = (typeof WEEKDAY_CODES)[number];

export const FREQUENCIES = [
  "none",
  "daily",
  "weekly",
  "monthly",
  "custom",
] as const;
export type Frequency = (typeof FREQUENCIES)[number];

export const MONTHLY_MODES = ["dayOfMonth", "weekdayOfMonth"] as const;
export type MonthlyMode = (typeof MONTHLY_MODES)[number];

export const RECURRENCE_ENDS = ["never", "onDate", "afterCount"] as const;
export type RecurrenceEnd = (typeof RECURRENCE_ENDS)[number];

export type RecurrenceSettings = {
  frequency: Frequency;
  interval: number;
  weekdays: WeekdayCode[];
  monthlyMode: MonthlyMode;
  ends: RecurrenceEnd;
  endDate: DateKey;
  count: number;
  customRule: string;
};

export type Occurrence = { start: Date; end: Date };

export type Schedule = {
  start: Date;
  durationMinutes: number;
  rule: RRule | null;
};

export type RuleResult = { ok: true; rule: RRule } | { ok: false; error: string };

export const MAX_OCCURRENCES = 500;

export const frequencyLabels: Record<Frequency, string> = {
  none: "Does not repeat",
  daily: "Daily",
  weekly: "Weekly",
  monthly: "Monthly",
  custom: "Custom (RRULE)",
};

export const intervalUnits: Partial<Record<Frequency, [string, string]>> = {
  daily: ["day", "days"],
  weekly: ["week", "weeks"],
  monthly: ["month", "months"],
};

const ordinalLabels: Record<number, string> = {
  1: "first",
  2: "second",
  3: "third",
  4: "fourth",
  [-1]: "last",
};

export function weekdayCodeOf(wallClock: Date): WeekdayCode {
  return WEEKDAY_CODES[(wallClock.getUTCDay() + 6) % 7];
}

export function weekdayName(code: WeekdayCode, width: "long" | "short" = "long") {
  const monday = Date.UTC(2024, 0, 1);
  const date = new Date(monday + WEEKDAY_CODES.indexOf(code) * 86_400_000);
  return new Intl.DateTimeFormat(undefined, {
    weekday: width,
    timeZone: "UTC",
  }).format(date);
}

export function weekdayOrdinalOf(wallClock: Date): number {
  const nth = Math.ceil(wallClock.getUTCDate() / 7);
  return nth > 4 ? -1 : nth;
}

export function describeMonthlyMode(mode: MonthlyMode, start: Date): string {
  if (mode === "dayOfMonth") return `On day ${start.getUTCDate()}`;
  return `On the ${ordinalLabels[weekdayOrdinalOf(start)]} ${weekdayName(weekdayCodeOf(start))}`;
}

export function normalizeRule(rule: string): string {
  return rule
    .trim()
    .replace(/^RRULE:/i, "")
    .toUpperCase();
}

export function buildRecurrenceRule(
  settings: RecurrenceSettings,
  start: Date,
): string | null {
  if (settings.frequency === "none") return null;
  if (settings.frequency === "custom") {
    return normalizeRule(settings.customRule) || null;
  }

  const parts = [`FREQ=${settings.frequency.toUpperCase()}`];

  if (settings.interval > 1) parts.push(`INTERVAL=${settings.interval}`);

  if (settings.frequency === "weekly") {
    const days = WEEKDAY_CODES.filter((code) => settings.weekdays.includes(code));
    parts.push(`BYDAY=${days.join(",")}`);
  }

  if (settings.frequency === "monthly") {
    parts.push(
      settings.monthlyMode === "dayOfMonth"
        ? `BYMONTHDAY=${start.getUTCDate()}`
        : `BYDAY=${weekdayOrdinalOf(start)}${weekdayCodeOf(start)}`,
    );
  }

  if (settings.ends === "afterCount") parts.push(`COUNT=${settings.count}`);

  return parts.join(";");
}

export function recurrenceEndOf(settings: RecurrenceSettings): Date | null {
  if (settings.frequency === "none" || settings.frequency === "custom") {
    return null;
  }
  return settings.ends === "onDate" && settings.endDate
    ? toWallClock(settings.endDate, "23:59")
    : null;
}

export function createRule(
  rule: string,
  start: Date,
  until: Date | null = null,
): RuleResult {
  const normalized = normalizeRule(rule);

  if (!/(^|;)FREQ=/.test(normalized)) {
    return {
      ok: false,
      error: "The rule needs a FREQ part, e.g. FREQ=WEEKLY;BYDAY=SU.",
    };
  }

  if (normalized.includes("DTSTART")) {
    return {
      ok: false,
      error: "Leave DTSTART out — the start date is used instead.",
    };
  }

  try {
    const options = RRule.parseString(normalized);
    return {
      ok: true,
      rule: new RRule({ ...options, dtstart: start, ...(until ? { until } : {}) }),
    };
  } catch {
    return { ok: false, error: "This is not a valid RRULE." };
  }
}

function withStart(starts: Date[], start: Date, from?: Date, to?: Date) {
  const inRange = (!from || start >= from) && (!to || start <= to);
  const alreadyIncluded = starts.some((date) => date.getTime() === start.getTime());

  return inRange && !alreadyIncluded
    ? [start, ...starts].sort((a, b) => a.getTime() - b.getTime())
    : starts;
}

function toOccurrences(starts: Date[], durationMinutes: number): Occurrence[] {
  return starts.map((start) => ({
    start,
    end: addMinutes(start, durationMinutes),
  }));
}

export function occurrencesBetween(
  { start, durationMinutes, rule }: Schedule,
  from: Date,
  to: Date,
): Occurrence[] {
  const searchFrom = addMinutes(from, -durationMinutes);
  const starts = rule
    ? rule.between(searchFrom, to, true, (_, index) => index < MAX_OCCURRENCES)
    : [];

  return toOccurrences(
    withStart(starts, start, searchFrom, to),
    durationMinutes,
  ).filter((occurrence) => occurrence.end > from);
}

export function firstOccurrences(
  { start, durationMinutes, rule }: Schedule,
  limit: number,
): Occurrence[] {
  const starts = rule ? rule.all((_, index) => index < limit) : [];
  return toOccurrences(withStart(starts, start).slice(0, limit), durationMinutes);
}

export function hasOverlap(occurrences: Occurrence[]): boolean {
  return occurrences.some(
    (occurrence, index) =>
      index > 0 && occurrences[index - 1].end > occurrence.start,
  );
}

export function describeRule(rule: RRule | null): string {
  if (!rule) return "Does not repeat";
  const text = rule.toText();
  return text.charAt(0).toUpperCase() + text.slice(1);
}

function ruleParts(rule: string): Map<string, string> {
  return new Map(
    normalizeRule(rule)
      .split(";")
      .filter(Boolean)
      .map((part) => {
        const [key, value = ""] = part.split("=");
        return [key, value] as const;
      }),
  );
}

function sameRule(a: string, b: string): boolean {
  const left = ruleParts(a);
  const right = ruleParts(b);
  return (
    left.size === right.size &&
    [...left].every(([key, value]) => right.get(key) === value)
  );
}

export function defaultRecurrenceSettings(start: Date): RecurrenceSettings {
  return {
    frequency: "none",
    interval: 1,
    weekdays: [weekdayCodeOf(start)],
    monthlyMode: "dayOfMonth",
    ends: "never",
    endDate: addMinutes(start, 90 * 24 * 60).toISOString().slice(0, 10),
    count: 10,
    customRule: "",
  };
}

export function recurrenceSettingsFromRule(
  rule: string | null,
  recurrenceEnd: Date | null,
  start: Date,
): RecurrenceSettings {
  const defaults = defaultRecurrenceSettings(start);

  if (!rule) return defaults;

  const parts = ruleParts(rule);
  const frequency = parts.get("FREQ")?.toLowerCase();
  const byDay = parts.get("BYDAY")?.split(",") ?? [];
  const count = parts.has("COUNT") ? Number(parts.get("COUNT")) : null;

  const candidate: RecurrenceSettings = {
    ...defaults,
    frequency:
      frequency === "daily" || frequency === "weekly" || frequency === "monthly"
        ? frequency
        : "custom",
    interval: parts.has("INTERVAL") ? Number(parts.get("INTERVAL")) : 1,
    weekdays: byDay.filter((code): code is WeekdayCode =>
      (WEEKDAY_CODES as readonly string[]).includes(code),
    ),
    monthlyMode: parts.has("BYDAY") ? "weekdayOfMonth" : "dayOfMonth",
    ends: count ? "afterCount" : recurrenceEnd ? "onDate" : "never",
    endDate: recurrenceEnd
      ? recurrenceEnd.toISOString().slice(0, 10)
      : defaults.endDate,
    count: count ?? defaults.count,
  };

  if (
    candidate.frequency !== "custom" &&
    sameRule(buildRecurrenceRule(candidate, start) ?? "", rule)
  ) {
    return candidate;
  }

  return {
    ...defaults,
    frequency: "custom",
    customRule: normalizeRule(rule),
  };
}
