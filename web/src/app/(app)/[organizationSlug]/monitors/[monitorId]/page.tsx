import { notFound } from "next/navigation";
import { Suspense } from "react";
import { requireOrganization } from "@/lib/auth";
import { isUuid } from "@/lib/utils";
import { MonitorDetailsView } from "@/modules/monitors/ui/details/monitor-details-view";

export default async function MonitorDetailsPage({
  params,
}: {
  params: Promise<{ organizationSlug: string; monitorId: string }>;
}) {
  const { organizationSlug, monitorId } = await params;

  // API przyjmuje tylko GUID — inny segment to od razu 404 zamiast zapytania skazanego na błąd
  if (!isUuid(monitorId)) {
    notFound();
  }

  const organization = await requireOrganization(organizationSlug);

  return (
    <div className="mx-auto flex w-full max-w-6xl flex-col gap-6 p-4">
      {/* useSearchParams (zakres wykresu) wymaga granicy Suspense */}
      <Suspense>
        <MonitorDetailsView
          monitorRef={{ organizationId: organization.id, monitorId }}
          monitorsHref={`/${organizationSlug}/monitors`}
          incidentsHref={`/${organizationSlug}/incidents`}
          notificationsHref={`/${organizationSlug}/notifications`}
        />
      </Suspense>
    </div>
  );
}
