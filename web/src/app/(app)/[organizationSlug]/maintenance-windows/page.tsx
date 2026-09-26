import { PlusIcon } from "lucide-react";
import Link from "next/link";
import { Suspense } from "react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { requireOrganization } from "@/lib/auth";
import { MaintenanceWindowsView } from "@/modules/maintenance-windows/ui/maintenance-windows-view";

export default async function MaintenanceWindowsPage({
  params,
}: {
  params: Promise<{ organizationSlug: string }>;
}) {
  const { organizationSlug } = await params;
  const organization = await requireOrganization(organizationSlug);
  const listHref = `/${organizationSlug}/maintenance-windows`;

  return (
    <div className="mx-auto flex w-full max-w-6xl flex-col gap-6 p-4">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div className="space-y-1">
          <h1 className="font-heading text-2xl">Maintenance windows</h1>
          <p className="text-muted-foreground text-sm">
            Planned work in {organization.name} that shouldn’t page anyone or
            count as downtime.
          </p>
        </div>
        <Button render={<Link href={`${listHref}/create`} />}>
          <PlusIcon />
          New maintenance window
        </Button>
      </div>

      <Suspense fallback={<Skeleton className="h-64 w-full rounded-xl" />}>
        <MaintenanceWindowsView
          organizationId={organization.id}
          listHref={listHref}
          monitorsHref={`/${organizationSlug}/monitors`}
        />
      </Suspense>
    </div>
  );
}
