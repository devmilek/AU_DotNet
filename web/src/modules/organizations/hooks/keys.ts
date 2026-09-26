export const organizationKeys = {
  all: ["organizations"] as const,
  list: () => [...organizationKeys.all, "list"] as const,
  detail: (organizationId: string) =>
    [...organizationKeys.all, "detail", organizationId] as const,
  members: (organizationId: string) =>
    [...organizationKeys.detail(organizationId), "members"] as const,
  invitations: (organizationId: string) =>
    [...organizationKeys.detail(organizationId), "invitations"] as const,
  invitation: (token: string) =>
    [...organizationKeys.all, "invitation", token] as const,
};
