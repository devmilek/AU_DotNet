import { CreateOrganizationForm } from "@/modules/organizations/ui/create-organization-form";
import { requireUser } from "@/lib/auth";
import Link from "next/link";

export default async function NewOrganizationPage() {
  await requireUser();

  return (
    <div className="flex flex-col gap-6">
      <div className="space-y-1.5 text-center">
        <h1 className="font-heading text-2xl">Create an organization</h1>
        <p className="text-muted-foreground text-sm">
          Set up your workspace to start monitoring.
        </p>
      </div>

      <CreateOrganizationForm />

      <p className="text-center text-muted-foreground text-sm">
        Already have one?{" "}
        <Link href="/organizations" className="text-foreground underline-offset-4 hover:underline">
          Choose organization
        </Link>
      </p>
    </div>
  );
}
