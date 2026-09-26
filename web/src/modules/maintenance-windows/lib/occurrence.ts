import type { components } from "@/lib/api/schema";

export type WindowOccurrence = components["schemas"]["WindowOccurrenceResponse"];

export type OccurrencePhase = "upcoming" | "inProgress" | "ended" | "cancelled";

export function occurrencePhase(
  occurrence: WindowOccurrence,
  now: Date,
): OccurrencePhase {
  if (occurrence.status === "Cancelled") return "cancelled";
  if (new Date(occurrence.endsAtUtc) <= now) return "ended";
  if (new Date(occurrence.startsAtUtc) <= now) return "inProgress";
  return "upcoming";
}

export function hasStarted(occurrence: WindowOccurrence, now: Date): boolean {
  return new Date(occurrence.startsAtUtc) <= now;
}

export function isRescheduled(occurrence: WindowOccurrence): boolean {
  return (
    new Date(occurrence.startsAtUtc).getTime() !==
    new Date(occurrence.scheduledStartUtc).getTime()
  );
}
