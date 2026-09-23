import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar";
import { AppSidebar } from "@/components/sidebar";
import { getOrganizations, requireUser } from "@/lib/auth";
import { redirect } from "next/navigation";

export default async function OrganizationLayout({
  children,
  params,
}: {
  children: React.ReactNode;
  params: Promise<{ organizationSlug: string }>;
}) {
  await requireUser();
  const { organizationSlug } = await params;
  const organizations = await getOrganizations();
  const organization = organizations.find(
    (item) => item.slug === organizationSlug,
  );

  if (!organization) {
    redirect("/organizations");
  }

  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <header className="flex h-12 items-center gap-2 px-4">
          <SidebarTrigger />
        </header>
        {children}
      </SidebarInset>
    </SidebarProvider>
  );
}
