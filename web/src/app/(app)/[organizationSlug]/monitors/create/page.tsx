import { ArrowLeftIcon } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { requireOrganization } from "@/lib/auth";
import { CreateHttpMonitorForm } from "@/modules/monitors/ui/create-http-monitor-form";

export default async function CreateMonitorPage({
  params,
}: {
  params: Promise<{ organizationSlug: string }>;
}) {
  const { organizationSlug } = await params;
  const organization = await requireOrganization(organizationSlug);

  return (
    <div className="mx-auto flex w-full max-w-3xl flex-col gap-6 p-4">
      <div className="flex flex-col items-start gap-3">
        <Button
          size="sm"
          variant="ghost"
          className="-ms-2"
          render={<Link href={`/${organizationSlug}/monitors`} />}
        >
          <ArrowLeftIcon />
          Monitors
        </Button>
        <div className="space-y-1">
          <h1 className="font-heading text-2xl">New HTTP monitor</h1>
          <p className="text-muted-foreground text-sm">
            We will request the URL on a schedule and alert you when it fails.
          </p>
        </div>
      </div>

      <CreateHttpMonitorForm
        organizationId={organization.id}
        organizationSlug={organizationSlug}
      />
    </div>
  );
}
