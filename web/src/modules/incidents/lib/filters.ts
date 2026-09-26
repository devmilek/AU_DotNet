import type { components } from "@/lib/api/schema";

export type IncidentStatusFilter = components["schemas"]["IncidentStatusFilter"];

export const statusFilters: { value: IncidentStatusFilter; label: string }[] = [
  { value: "All", label: "All" },
  { value: "Ongoing", label: "Ongoing" },
  { value: "Resolved", label: "Resolved" },
];

export function parseStatusFilter(value: string | null): IncidentStatusFilter {
  const match = statusFilters.find(
    (filter) => filter.value.toLowerCase() === value?.toLowerCase(),
  );
  return match?.value ?? "All";
}

export function parsePage(value: string | null): number {
  const page = Number(value);
  return Number.isInteger(page) && page > 0 ? page : 1;
}
