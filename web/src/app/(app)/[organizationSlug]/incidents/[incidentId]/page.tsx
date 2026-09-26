import { notFound } from "next/navigation";
import { requireOrganization } from "@/lib/auth";
import { isUuid } from "@/lib/utils";
import { IncidentDetailsView } from "@/modules/incidents/ui/details/incident-details-view";
import { canManageMembers } from "@/modules/organizations/lib/roles";

export default async function IncidentPage({
  params,
}: {
  params: Promise<{ organizationSlug: string; incidentId: string }>;
}) {
  const { organizationSlug, incidentId } = await params;

  if (!isUuid(incidentId)) {
    notFound();
  }

  const organization = await requireOrganization(organizationSlug);

  return (
    <div className="mx-auto flex w-full max-w-6xl flex-col gap-6 p-4">
      <IncidentDetailsView
        incidentRef={{ organizationId: organization.id, incidentId }}
        listHref={`/${organizationSlug}/incidents`}
        monitorsHref={`/${organizationSlug}/monitors`}
        canManage={canManageMembers(organization.role)}
      />
    </div>
  );
}
