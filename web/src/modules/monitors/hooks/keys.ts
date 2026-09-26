import type { MonitorsParams } from "@/modules/monitors/lib/search-params";
import type { ResponseTimeRange } from "@/modules/monitors/lib/response-time-range";

// hierarchia kluczy: unieważnienie `detail` odświeża też status, uptime i wykres tego monitora
export const monitorKeys = {
  all: ["monitors"] as const,
  organization: (organizationId: string) =>
    [...monitorKeys.all, organizationId] as const,
  lists: (organizationId: string) =>
    [...monitorKeys.organization(organizationId), "list"] as const,
  list: (organizationId: string, params: MonitorsParams) =>
    [...monitorKeys.lists(organizationId), params] as const,
  options: (organizationId: string) =>
    [...monitorKeys.organization(organizationId), "options"] as const,
  statuses: (organizationId: string) =>
    [...monitorKeys.organization(organizationId), "statuses"] as const,
  detail: (organizationId: string, monitorId: string) =>
    [...monitorKeys.organization(organizationId), "detail", monitorId] as const,
  status: (organizationId: string, monitorId: string) =>
    [...monitorKeys.detail(organizationId, monitorId), "status"] as const,
  uptime: (organizationId: string, monitorId: string) =>
    [...monitorKeys.detail(organizationId, monitorId), "uptime"] as const,
  responseTimes: (
    organizationId: string,
    monitorId: string,
    range: ResponseTimeRange,
  ) =>
    [
      ...monitorKeys.detail(organizationId, monitorId),
      "response-times",
      range,
    ] as const,
};

/** Identyfikacja monitora w ścieżkach API. */
export type MonitorRef = { organizationId: string; monitorId: string };
