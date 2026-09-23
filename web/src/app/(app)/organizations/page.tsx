import { Button } from "@/components/ui/button";
import { getOrganizations, requireUser } from "@/lib/auth";
import { organizationRoleLabel } from "@/modules/organizations/lib/role-label";
import { resolveOrganizationsHome } from "@/modules/organizations/lib/resolve-home";
import Link from "next/link";
import { redirect } from "next/navigation";

export default async function OrganizationsPage() {
  await requireUser();
  const organizations = await getOrganizations();

  if (organizations.length < 2) {
    redirect(resolveOrganizationsHome(organizations));
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="space-y-1.5 text-center">
        <h1 className="font-heading text-2xl">Choose an organization</h1>
        <p className="text-muted-foreground text-sm">
          Select where you want to continue.
        </p>
      </div>

      <ul className="flex flex-col gap-2">
        {organizations.map((organization) => (
          <li key={organization.id}>
            <Link
              href={`/${organization.slug}/monitors`}
              className="flex items-center gap-3 rounded-xl border px-4 py-3 transition-colors hover:bg-accent"
            >
              <div className="flex size-9 items-center justify-center rounded-lg bg-sidebar-primary text-sm font-medium text-sidebar-primary-foreground">
                {organization.name.slice(0, 1).toUpperCase()}
              </div>
              <div className="min-w-0 flex-1 text-left">
                <div className="truncate font-medium">{organization.name}</div>
                <div className="truncate text-muted-foreground text-xs">
                  {organizationRoleLabel(organization.role)}
                </div>
              </div>
            </Link>
          </li>
        ))}
      </ul>

      <Button variant="outline" render={<Link href="/organizations/new" />}>
        Create organization
      </Button>
    </div>
  );
}
