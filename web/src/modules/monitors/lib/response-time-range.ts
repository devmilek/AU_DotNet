import type { components } from "@/lib/api/schema";

export type ResponseTimeRange = components["schemas"]["ResponseTimeRange"];

/** Krótkie wartości do URL (`?range=7d`) ↔ enum API. */
export const responseTimeRanges = [
  { param: "24h", value: "Last24Hours", label: "Last 24 hours" },
  { param: "7d", value: "Last7Days", label: "Last 7 days" },
  { param: "30d", value: "Last30Days", label: "Last 30 days" },
] as const satisfies readonly {
  param: string;
  value: ResponseTimeRange;
  label: string;
}[];

export const defaultResponseTimeRange: ResponseTimeRange = "Last24Hours";

export function parseResponseTimeRange(
  param: string | null,
): ResponseTimeRange {
  return (
    responseTimeRanges.find((range) => range.param === param)?.value ??
    defaultResponseTimeRange
  );
}

export function responseTimeRangeParam(range: ResponseTimeRange): string {
  return responseTimeRanges.find((r) => r.value === range)!.param;
}

export function responseTimeRangeLabel(range: ResponseTimeRange): string {
  return responseTimeRanges.find((r) => r.value === range)!.label;
}
