import { PlusIcon } from "lucide-react";
import Link from "next/link";
import { Suspense } from "react";
import { Button } from "@/components/ui/button";
import { requireOrganization } from "@/lib/auth";
import { MonitorsListSkeleton } from "@/modules/monitors/ui/monitors-list";
import { MonitorsView } from "@/modules/monitors/ui/monitors-view";

export default async function MonitorsPage({
  params,
}: {
  params: Promise<{ organizationSlug: string }>;
}) {
  const { organizationSlug } = await params;
  const organization = await requireOrganization(organizationSlug);

  return (
    <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-4">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div className="space-y-1">
          <h1 className="font-heading text-2xl">Monitors</h1>
          <p className="text-muted-foreground text-sm">
            Everything you are watching in {organization.name}.
          </p>
        </div>
        <Button render={<Link href={`/${organizationSlug}/monitors/create`} />}>
          <PlusIcon />
          New monitor
        </Button>
      </div>

      <Suspense fallback={<MonitorsListSkeleton />}>
        <MonitorsView organizationId={organization.id} />
      </Suspense>
    </div>
  );
}
