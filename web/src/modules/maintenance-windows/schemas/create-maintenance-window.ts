import { formOptions, revalidateLogic } from "@tanstack/react-form";
import { z } from "zod";
import type { components } from "@/lib/api/schema";
import {
  buildRecurrenceRule,
  createRule,
  FREQUENCIES,
  MAX_OCCURRENCES,
  MONTHLY_MODES,
  RECURRENCE_ENDS,
  recurrenceEndOf,
  recurrenceSettingsFromRule,
  WEEKDAY_CODES,
  weekdayCodeOf,
} from "../lib/recurrence";
import { browserTimeZone } from "../lib/time-zones";
import {
  addDays,
  localDateKey,
  parseLocalDateTime,
  toLocalDateTimeString,
  toWallClock,
} from "../lib/wall-clock";

export const MAX_NAME_LENGTH = 200;
export const MAX_DESCRIPTION_LENGTH = 2000;
export const MAX_DURATION_MINUTES = 30 * 24 * 60;

const datePattern = /^\d{4}-\d{2}-\d{2}$/;
const timePattern = /^([01]\d|2[0-3]):[0-5]\d$/;

const recurrenceSchema = z.object({
  frequency: z.enum(FREQUENCIES),
  interval: z
    .number("Enter a number.")
    .int()
    .min(1, "At least 1.")
    .max(99, "At most 99."),
  weekdays: z.array(z.enum(WEEKDAY_CODES)),
  monthlyMode: z.enum(MONTHLY_MODES),
  ends: z.enum(RECURRENCE_ENDS),
  endDate: z.string(),
  count: z
    .number("Enter a number.")
    .int()
    .min(1, "At least 1 occurrence.")
    .max(MAX_OCCURRENCES, `At most ${MAX_OCCURRENCES} occurrences.`),
  customRule: z.string(),
});

const scheduleSchema = z
  .object({
    timeZoneId: z.string().min(1, "Pick a time zone."),
    startDate: z.string().regex(datePattern, "Pick a start date."),
    startTime: z.string().regex(timePattern, "Enter a start time."),
    durationHours: z.number("Enter hours.").int().min(0),
    durationMinutes: z.number("Enter minutes.").int().min(0).max(59),
    recurrence: recurrenceSchema,
  })
  .superRefine((schedule, ctx) => {
    const total = schedule.durationHours * 60 + schedule.durationMinutes;

    if (total < 1) {
      ctx.addIssue({
        code: "custom",
        path: ["durationMinutes"],
        message: "The window must last at least 1 minute.",
      });
    }

    if (total > MAX_DURATION_MINUTES) {
      ctx.addIssue({
        code: "custom",
        path: ["durationHours"],
        message: "The window can last at most 30 days.",
      });
    }

    const { recurrence } = schedule;

    if (recurrence.frequency === "weekly" && recurrence.weekdays.length === 0) {
      ctx.addIssue({
        code: "custom",
        path: ["recurrence", "weekdays"],
        message: "Pick at least one day.",
      });
    }

    if (recurrence.frequency === "custom") {
      const start = toWallClock(schedule.startDate, schedule.startTime);
      const result = createRule(recurrence.customRule, start);
      if (!result.ok) {
        ctx.addIssue({
          code: "custom",
          path: ["recurrence", "customRule"],
          message: result.error,
        });
      }
    }

    if (
      (recurrence.frequency === "daily" ||
        recurrence.frequency === "weekly" ||
        recurrence.frequency === "monthly") &&
      recurrence.ends === "onDate" &&
      !(datePattern.test(recurrence.endDate) && recurrence.endDate >= schedule.startDate)
    ) {
      ctx.addIssue({
        code: "custom",
        path: ["recurrence", "endDate"],
        message: "Pick a date on or after the start date.",
      });
    }
  });

export const createMaintenanceWindowSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Name is required.")
    .max(MAX_NAME_LENGTH, `Name must be at most ${MAX_NAME_LENGTH} characters.`),
  description: z
    .string()
    .trim()
    .max(
      MAX_DESCRIPTION_LENGTH,
      `Description must be at most ${MAX_DESCRIPTION_LENGTH} characters.`,
    ),
  schedule: scheduleSchema,
  monitorIds: z.array(z.string()).min(1, "Pick at least one monitor."),
  suppressNotifications: z.boolean(),
  excludeFromSla: z.boolean(),
});

export type CreateMaintenanceWindowValues = z.input<
  typeof createMaintenanceWindowSchema
>;
export type ScheduleValues = CreateMaintenanceWindowValues["schedule"];

export const maintenanceWindowFormOptions = formOptions({
  defaultValues: {} as CreateMaintenanceWindowValues,
  validationLogic: revalidateLogic({
    mode: "submit",
    modeAfterSubmission: "change",
  }),
  validators: { onDynamic: createMaintenanceWindowSchema },
});

export function createMaintenanceWindowDefaults(
  now = new Date(),
): CreateMaintenanceWindowValues {
  const tomorrow = new Date(now);
  tomorrow.setDate(now.getDate() + 1);
  const startDate = localDateKey(tomorrow);
  const startTime = "02:00";

  return {
    name: "",
    description: "",
    schedule: {
      timeZoneId: browserTimeZone(),
      startDate,
      startTime,
      durationHours: 1,
      durationMinutes: 0,
      recurrence: {
        frequency: "weekly",
        interval: 1,
        weekdays: [weekdayCodeOf(toWallClock(startDate, startTime))],
        monthlyMode: "dayOfMonth",
        ends: "never",
        endDate: addDays(startDate, 90),
        count: 10,
        customRule: "",
      },
    },
    monitorIds: [],
    suppressNotifications: true,
    excludeFromSla: true,
  };
}

export function durationOf(schedule: ScheduleValues): number {
  return schedule.durationHours * 60 + schedule.durationMinutes;
}

export function toCreateMaintenanceWindowRequest(
  values: CreateMaintenanceWindowValues,
): components["schemas"]["CreateMaintenanceWindowRequest"] {
  const { schedule } = values;
  const start = toWallClock(schedule.startDate, schedule.startTime);
  const recurrenceEnd = recurrenceEndOf(schedule.recurrence);

  return {
    name: values.name.trim(),
    description: values.description.trim() || null,
    schedule: {
      timeZoneId: schedule.timeZoneId,
      startsAtLocal: toLocalDateTimeString(start),
      durationMinutes: durationOf(schedule),
      recurrenceRule: buildRecurrenceRule(schedule.recurrence, start),
      recurrenceEndLocal: recurrenceEnd
        ? toLocalDateTimeString(recurrenceEnd)
        : null,
    },
    monitorIds: values.monitorIds,
    suppressNotifications: values.suppressNotifications,
    excludeFromSla: values.excludeFromSla,
  };
}

type MaintenanceWindow = components["schemas"]["MaintenanceWindowResponse"];
type ScheduleInput = components["schemas"]["MaintenanceScheduleInput"];

export function maintenanceWindowToFormValues(
  maintenanceWindow: MaintenanceWindow,
): CreateMaintenanceWindowValues {
  const start = parseLocalDateTime(maintenanceWindow.startsAtLocal);
  const recurrenceEnd = maintenanceWindow.recurrenceEndLocal
    ? parseLocalDateTime(maintenanceWindow.recurrenceEndLocal)
    : null;
  const localDateTime = toLocalDateTimeString(start);

  return {
    name: maintenanceWindow.name,
    description: maintenanceWindow.description ?? "",
    schedule: {
      timeZoneId: maintenanceWindow.timeZoneId,
      startDate: localDateTime.slice(0, 10),
      startTime: localDateTime.slice(11, 16),
      durationHours: Math.floor(maintenanceWindow.durationMinutes / 60),
      durationMinutes: maintenanceWindow.durationMinutes % 60,
      recurrence: recurrenceSettingsFromRule(
        maintenanceWindow.recurrenceRule,
        recurrenceEnd,
        start,
      ),
    },
    monitorIds: maintenanceWindow.monitors.map((monitor) => monitor.id),
    suppressNotifications: maintenanceWindow.suppressNotifications,
    excludeFromSla: maintenanceWindow.excludeFromSla,
  };
}

export type MaintenanceWindowChanges = {
  content: { name: string; description: string | null } | null;
  schedule: ScheduleInput | null;
  policy: { suppressNotifications: boolean; excludeFromSla: boolean } | null;
  monitorIds: string[] | null;
};

function sameSchedule(a: ScheduleInput, b: ScheduleInput) {
  return (
    a.timeZoneId === b.timeZoneId &&
    a.startsAtLocal === b.startsAtLocal &&
    a.durationMinutes === b.durationMinutes &&
    (a.recurrenceRule ?? null) === (b.recurrenceRule ?? null) &&
    (a.recurrenceEndLocal ?? null) === (b.recurrenceEndLocal ?? null)
  );
}

function sameIds(a: string[], b: string[]) {
  return a.length === b.length && a.every((id) => b.includes(id));
}

export function diffMaintenanceWindow(
  initial: CreateMaintenanceWindowValues,
  current: CreateMaintenanceWindowValues,
): MaintenanceWindowChanges {
  const before = toCreateMaintenanceWindowRequest(initial);
  const after = toCreateMaintenanceWindowRequest(current);

  return {
    content:
      before.name !== after.name || before.description !== after.description
        ? { name: after.name, description: after.description ?? null }
        : null,
    schedule: sameSchedule(before.schedule, after.schedule)
      ? null
      : after.schedule,
    policy:
      before.suppressNotifications !== after.suppressNotifications ||
      before.excludeFromSla !== after.excludeFromSla
        ? {
            suppressNotifications: after.suppressNotifications,
            excludeFromSla: after.excludeFromSla,
          }
        : null,
    monitorIds: sameIds(before.monitorIds, after.monitorIds)
      ? null
      : after.monitorIds,
  };
}

export function hasChanges(changes: MaintenanceWindowChanges): boolean {
  return Object.values(changes).some((change) => change !== null);
}
