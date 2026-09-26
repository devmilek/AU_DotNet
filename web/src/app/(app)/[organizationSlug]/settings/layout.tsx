import { requireOrganization } from "@/lib/auth";
import { SettingsNav } from "@/modules/organizations/ui/settings/settings-nav";

export default async function SettingsLayout({
  children,
  params,
}: {
  children: React.ReactNode;
  params: Promise<{ organizationSlug: string }>;
}) {
  const { organizationSlug } = await params;
  const organization = await requireOrganization(organizationSlug);

  return (
    <div className="mx-auto flex w-full max-w-4xl flex-col gap-6 p-4">
      <div className="space-y-1">
        <h1 className="font-heading text-2xl">Settings</h1>
        <p className="text-muted-foreground text-sm">
          Manage {organization.name} and the people who can access it.
        </p>
      </div>
      <SettingsNav settingsHref={`/${organizationSlug}/settings`} />
      {children}
    </div>
  );
}
