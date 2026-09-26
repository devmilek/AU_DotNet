export const channelKeys = {
  all: ["channels"] as const,
  organization: (organizationId: string) =>
    [...channelKeys.all, organizationId] as const,
  list: (organizationId: string) =>
    [...channelKeys.organization(organizationId), "list"] as const,
  detail: (organizationId: string, channelId: string) =>
    [...channelKeys.organization(organizationId), "detail", channelId] as const,
  forMonitor: (organizationId: string, monitorId: string) =>
    [...channelKeys.organization(organizationId), "monitor", monitorId] as const,
};

export type ChannelRef = { organizationId: string; channelId: string };
