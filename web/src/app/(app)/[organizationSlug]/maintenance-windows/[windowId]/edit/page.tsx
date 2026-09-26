import { notFound } from "next/navigation";
import { requireOrganization } from "@/lib/auth";
import { isUuid } from "@/lib/utils";
import { EditMaintenanceWindowView } from "@/modules/maintenance-windows/ui/details/edit-maintenance-window-view";

export default async function EditMaintenanceWindowPage({
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
    <div className="mx-auto flex w-full max-w-7xl flex-col gap-6 p-4">
      <EditMaintenanceWindowView
        windowRef={{ organizationId: organization.id, windowId }}
        detailsHref={`/${organizationSlug}/maintenance-windows/${windowId}`}
      />
    </div>
  );
}
