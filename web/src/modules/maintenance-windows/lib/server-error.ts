import { ApiError } from "@/lib/api/errors";

export type ServerError = { title: string; details: string[] };

const serverFieldLabels: Record<string, string> = {
  Name: "Name",
  Description: "Description",
  MonitorIds: "Monitors",
  Schedule: "Schedule",
  "Schedule.TimeZoneId": "Time zone",
  "Schedule.StartsAtLocal": "Start",
  "Schedule.DurationMinutes": "Duration",
  "Schedule.RecurrenceRule": "Repeat",
  "Schedule.RecurrenceEndLocal": "Ends",
  TimeZoneId: "Time zone",
  StartsAtLocal: "Start",
  DurationMinutes: "Duration",
  RecurrenceRule: "Repeat",
  RecurrenceEndLocal: "Ends",
};

export function toServerError(error: unknown): ServerError {
  if (!(error instanceof ApiError)) {
    return { title: "Something went wrong. Please try again.", details: [] };
  }

  if (error.status === 403) {
    return {
      title: "You need admin permissions in this organization.",
      details: [],
    };
  }

  const errors = (error.problem as { errors?: Record<string, string[]> })
    ?.errors;

  return {
    title: error.message,
    details: Object.entries(errors ?? {}).flatMap(([key, messages]) =>
      messages.map((message) => `${serverFieldLabels[key] ?? key}: ${message}`),
    ),
  };
}
