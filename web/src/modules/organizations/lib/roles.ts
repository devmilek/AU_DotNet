import type { components } from "@/lib/api/schema";

export type OrganizationRole = components["schemas"]["OrganizationRole"];

export const ROLE = { Member: 0, Admin: 1, Owner: 2 } as const;

export const roleOptions: {
  value: OrganizationRole;
  label: string;
  description: string;
}[] = [
  {
    value: ROLE.Member,
    label: "Member",
    description: "Sees monitors, incidents and maintenance windows.",
  },
  {
    value: ROLE.Admin,
    label: "Admin",
    description: "Manages monitors, notifications and members.",
  },
  {
    value: ROLE.Owner,
    label: "Owner",
    description: "Full access, including deleting the organization.",
  },
];

export function canManageMembers(role: OrganizationRole) {
  return role >= ROLE.Admin;
}

export function canManage(actorRole: OrganizationRole, targetRole: OrganizationRole) {
  return canManageMembers(actorRole) && targetRole <= actorRole;
}

export function assignableRoles(actorRole: OrganizationRole) {
  return roleOptions.filter((option) => option.value <= actorRole);
}
