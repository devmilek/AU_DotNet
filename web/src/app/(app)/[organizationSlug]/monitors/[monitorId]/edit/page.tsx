import { notFound } from "next/navigation";
import { requireOrganization } from "@/lib/auth";
import { isUuid } from "@/lib/utils";
import { EditHttpMonitorView } from "@/modules/monitors/ui/edit-http-monitor-view";

export default async function EditMonitorPage({
  params,
}: {
  params: Promise<{ organizationSlug: string; monitorId: string }>;
}) {
  const { organizationSlug, monitorId } = await params;

  if (!isUuid(monitorId)) {
    notFound();
  }

  const organization = await requireOrganization(organizationSlug);

  return (
    <div className="mx-auto flex w-full max-w-3xl flex-col gap-6 p-4">
      <EditHttpMonitorView
        monitorRef={{ organizationId: organization.id, monitorId }}
        monitorHref={`/${organizationSlug}/monitors/${monitorId}`}
      />
    </div>
  );
}
