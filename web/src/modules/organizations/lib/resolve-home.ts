import type { components } from "@/lib/api/schema";

export type Organization = components["schemas"]["OrganizationResponse"];

export function resolveOrganizationsHome(organizations: Organization[]) {
  if (organizations.length === 0) return "/organizations/new";
  if (organizations.length === 1) {
    return `/${organizations[0]!.slug}/monitors`;
  }
  return "/organizations";
}
