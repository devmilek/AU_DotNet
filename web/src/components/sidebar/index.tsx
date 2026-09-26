"use client";

import {
  Activity,
  BellIcon,
  CalendarClockIcon,
  SettingsIcon,
  SirenIcon,
} from "lucide-react";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarRail,
} from "@/components/ui/sidebar";
import { NavMain } from "./nav-main";
import { OrganizationSwitcher } from "./organization-switcher";
import { NavUser } from "./nav-user";
import { useParams, usePathname } from "next/navigation";
import { useOrganizations } from "@/modules/organizations/hooks/use-organizations";
import { SidebarMonitors } from "@/modules/monitors/ui/sidebar-monitors";

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const params = useParams<{ organizationSlug: string }>();
  const pathname = usePathname();
  const slug = params.organizationSlug;
  const organizations = useOrganizations();
  const organization = organizations.data?.find((item) => item.slug === slug);

  const navMain = [
    { title: "Monitors", url: `/${slug}/monitors`, icon: Activity },
    { title: "Incidents", url: `/${slug}/incidents`, icon: SirenIcon },
    {
      title: "Notifications",
      url: `/${slug}/notifications`,
      icon: BellIcon,
    },
    {
      title: "Maintenance windows",
      url: `/${slug}/maintenance-windows`,
      icon: CalendarClockIcon,
    },
    { title: "Settings", url: `/${slug}/settings`, icon: SettingsIcon },
  ].map((item) => ({
    ...item,
    // aktywna także na podstronach (szczegóły, tworzenie)
    isActive: pathname === item.url || pathname.startsWith(`${item.url}/`),
  }));

  return (
    <Sidebar collapsible="icon" variant="floating" {...props}>
      <SidebarHeader>
        <OrganizationSwitcher />
      </SidebarHeader>
      <SidebarContent>
        <NavMain items={navMain} />
        {organization ? (
          <SidebarMonitors
            organizationId={organization.id}
            organizationSlug={slug}
          />
        ) : null}
      </SidebarContent>
      <SidebarFooter>
        <NavUser />
      </SidebarFooter>
      <SidebarRail />
    </Sidebar>
  );
}
