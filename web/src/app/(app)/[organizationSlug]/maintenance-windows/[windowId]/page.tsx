import { notFound } from "next/navigation";
import { requireOrganization } from "@/lib/auth";
import { isUuid } from "@/lib/utils";
import { MaintenanceWindowDetailsView } from "@/modules/maintenance-windows/ui/details/maintenance-window-details-view";

export default async function MaintenanceWindowPage({
  params,
}: {
  params: Promise<{ organizationSlug: string; windowId: string }>;
}) {
  const { organizationSlug, windowId } = await params;

  if (!isUuid(windowId)) {
    notFound();
  }

  const organization = await requireOrganization(organizationSlug);

  return (
    <div className="mx-auto flex w-full max-w-6xl flex-col gap-6 p-4">
      <MaintenanceWindowDetailsView
        windowRef={{ organizationId: organization.id, windowId }}
        listHref={`/${organizationSlug}/maintenance-windows`}
        monitorsHref={`/${organizationSlug}/monitors`}
      />
    </div>
  );
}
