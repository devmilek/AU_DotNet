import type { IncidentStatusFilter } from "../lib/filters";

export const incidentKeys = {
  all: ["incidents"] as const,
  organization: (organizationId: string) =>
    [...incidentKeys.all, organizationId] as const,
  list: (organizationId: string, params: IncidentListParams) =>
    [...incidentKeys.organization(organizationId), "list", params] as const,
  detail: (organizationId: string, incidentId: string) =>
    [...incidentKeys.organization(organizationId), "detail", incidentId] as const,
};

export type IncidentListParams = {
  status: IncidentStatusFilter;
  page: number;
  pageSize?: number;
  monitorId?: string;
};

export type IncidentRef = { organizationId: string; incidentId: string };
