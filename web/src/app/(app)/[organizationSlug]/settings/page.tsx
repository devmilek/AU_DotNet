import { requireOrganization } from "@/lib/auth";
import { GeneralSettings } from "@/modules/organizations/ui/settings/general-settings";

export default async function GeneralSettingsPage({
  params,
}: {
  params: Promise<{ organizationSlug: string }>;
}) {
  const { organizationSlug } = await params;
  const organization = await requireOrganization(organizationSlug);

  return <GeneralSettings organizationId={organization.id} />;
}
