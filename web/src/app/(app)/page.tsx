import { requireUser, getOrganizations } from "@/lib/auth";
import { resolveOrganizationsHome } from "@/modules/organizations/lib/resolve-home";
import { redirect } from "next/navigation";

export default async function AppHomePage() {
  await requireUser();
  const organizations = await getOrganizations();
  redirect(resolveOrganizationsHome(organizations));
}
