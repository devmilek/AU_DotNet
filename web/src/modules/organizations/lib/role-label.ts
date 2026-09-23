import type { components } from "@/lib/api/schema";

type OrganizationRole = components["schemas"]["OrganizationRole"];

const ROLE_LABELS: Record<number, string> = {
  0: "Member",
  1: "Admin",
  2: "Owner",
};

export function organizationRoleLabel(role: OrganizationRole) {
  return ROLE_LABELS[Number(role)] ?? "Member";
}
