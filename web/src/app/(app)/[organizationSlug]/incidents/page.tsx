import { Suspense } from "react";
import { Skeleton } from "@/components/ui/skeleton";
import { requireOrganization } from "@/lib/auth";
import { IncidentsList } from "@/modules/incidents/ui/incidents-list";
import { canManageMembers } from "@/modules/organizations/lib/roles";

export default async function IncidentsPage({
  params,
}: {
  params: Promise<{ organizationSlug: string }>;
}) {
  const { organizationSlug } = await params;
  const organization = await requireOrganization(organizationSlug);

  return (
    <div className="mx-auto flex w-full max-w-6xl flex-col gap-6 p-4">
      <div className="space-y-1">
        <h1 className="font-heading text-2xl">Incidents</h1>
        <p className="text-muted-foreground text-sm">
          Outages detected across monitors in {organization.name}.
        </p>
      </div>

      <Suspense fallback={<Skeleton className="h-64 w-full rounded-xl" />}>
        <IncidentsList
          organizationId={organization.id}
          listHref={`/${organizationSlug}/incidents`}
          monitorsHref={`/${organizationSlug}/monitors`}
          canManage={canManageMembers(organization.role)}
        />
      </Suspense>
    </div>
  );
}
