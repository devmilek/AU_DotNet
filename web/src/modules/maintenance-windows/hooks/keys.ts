export const maintenanceWindowKeys = {
  all: ["maintenance-windows"] as const,
  organization: (organizationId: string) =>
    [...maintenanceWindowKeys.all, organizationId] as const,
  list: (organizationId: string) =>
    [...maintenanceWindowKeys.organization(organizationId), "list"] as const,
  detail: (organizationId: string, windowId: string) =>
    [
      ...maintenanceWindowKeys.organization(organizationId),
      "detail",
      windowId,
    ] as const,
  windowOccurrences: (
    organizationId: string,
    windowId: string,
    from: string,
    to: string,
  ) =>
    [
      ...maintenanceWindowKeys.detail(organizationId, windowId),
      "occurrences",
      from,
      to,
    ] as const,
  occurrences: (organizationId: string, from: string, to: string) =>
    [
      ...maintenanceWindowKeys.organization(organizationId),
      "occurrences",
      from,
      to,
    ] as const,
};

export type MaintenanceWindowRef = { organizationId: string; windowId: string };
