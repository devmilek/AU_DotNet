import { ArrowLeftIcon } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { requireOrganization } from "@/lib/auth";
import { CreateMaintenanceWindowForm } from "@/modules/maintenance-windows/ui/form/create-maintenance-window-form";

export default async function CreateMaintenanceWindowPage({
  params,
}: {
  params: Promise<{ organizationSlug: string }>;
}) {
  const { organizationSlug } = await params;
  const organization = await requireOrganization(organizationSlug);

  return (
    <div className="mx-auto flex w-full max-w-7xl flex-col gap-6 p-4">
      <div className="flex flex-col items-start gap-3">
        <Button
          size="sm"
          variant="ghost"
          className="-ms-2"
          render={<Link href={`/${organizationSlug}/maintenance-windows`} />}
        >
          <ArrowLeftIcon />
          Maintenance windows
        </Button>
        <div className="space-y-1">
          <h1 className="font-heading text-2xl">New maintenance window</h1>
          <p className="text-muted-foreground text-sm">
            Pick when it happens and which monitors it covers — the calendar
            shows every occurrence as you go.
          </p>
        </div>
      </div>

      <CreateMaintenanceWindowForm
        organizationId={organization.id}
        organizationSlug={organizationSlug}
      />
    </div>
  );
}
