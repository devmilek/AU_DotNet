import type { components } from "@/lib/api/schema";
import {
  durationOf,
  type ScheduleValues,
} from "../schemas/create-maintenance-window";
import {
  buildRecurrenceRule,
  createRule,
  recurrenceEndOf,
  type Schedule,
} from "./recurrence";
import { formatDate } from "./format";
import { parseLocalDateTime, toWallClock } from "./wall-clock";

type WindowSchedule = Pick<
  components["schemas"]["MaintenanceWindowResponse"],
  "startsAtLocal" | "durationMinutes" | "recurrenceRule" | "recurrenceEndLocal"
>;

export type ScheduleResult =
  | { ok: true; schedule: Schedule; rule: string | null }
  | { ok: false; error: string };

const datePattern = /^\d{4}-\d{2}-\d{2}$/;
const timePattern = /^\d{2}:\d{2}$/;

export function scheduleFromValues(values: ScheduleValues): ScheduleResult {
  if (!datePattern.test(values.startDate) || !timePattern.test(values.startTime)) {
    return { ok: false, error: "Pick a start date and time to see the preview." };
  }

  const durationMinutes = durationOf(values);
  if (!Number.isFinite(durationMinutes) || durationMinutes < 1) {
    return { ok: false, error: "Set a duration to see the preview." };
  }

  const start = toWallClock(values.startDate, values.startTime);
  const rule = buildRecurrenceRule(values.recurrence, start);

  if (!rule) return { ok: true, schedule: { start, durationMinutes, rule: null }, rule };

  const parsed = createRule(rule, start, recurrenceEndOf(values.recurrence));
  return parsed.ok
    ? { ok: true, schedule: { start, durationMinutes, rule: parsed.rule }, rule }
    : { ok: false, error: parsed.error };
}

export function scheduleOfWindow(window: WindowSchedule): Schedule {
  const start = parseLocalDateTime(window.startsAtLocal);
  const until = window.recurrenceEndLocal
    ? parseLocalDateTime(window.recurrenceEndLocal)
    : null;
  const parsed = window.recurrenceRule
    ? createRule(window.recurrenceRule, start, until)
    : null;

  return {
    start,
    durationMinutes: window.durationMinutes,
    rule: parsed?.ok ? parsed.rule : null,
  };
}

export function describeSeriesEnd(window: WindowSchedule): string {
  if (!window.recurrenceRule) return "Happens once";

  if (window.recurrenceEndLocal) {
    return `Until ${formatDate(parseLocalDateTime(window.recurrenceEndLocal), "UTC")}`;
  }

  const count = /(?:^|;)COUNT=(\d+)/i.exec(window.recurrenceRule)?.[1];
  if (count) return `After ${count} ${count === "1" ? "occurrence" : "occurrences"}`;

  return "No end date";
}
