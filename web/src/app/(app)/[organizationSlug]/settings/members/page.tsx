import { requireOrganization } from "@/lib/auth";
import { MembersSettings } from "@/modules/organizations/ui/settings/members-settings";

export default async function MembersSettingsPage({
  params,
}: {
  params: Promise<{ organizationSlug: string }>;
}) {
  const { organizationSlug } = await params;
  const organization = await requireOrganization(organizationSlug);

  return <MembersSettings organizationId={organization.id} />;
}
