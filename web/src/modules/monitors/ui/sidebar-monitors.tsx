"use client";

import {
  EllipsisIcon,
  ExternalLinkIcon,
  PauseIcon,
  PlayIcon,
  PlusIcon,
} from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { Menu, MenuItem, MenuLinkItem, MenuPopup, MenuTrigger } from "@/components/ui/menu";
import {
  SidebarGroup,
  SidebarGroupAction,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuAction,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSkeleton,
} from "@/components/ui/sidebar";
import type { components } from "@/lib/api/schema";
import { useSetMonitorPaused } from "../hooks/use-monitor-actions";
import { useMonitorStatuses } from "../hooks/use-monitor-statuses";
import { StatusDot } from "./status-dot";

type MonitorStatusSummary = components["schemas"]["MonitorStatusSummaryResponse"];

export function SidebarMonitors({
  organizationId,
  organizationSlug,
}: {
  organizationId: string;
  organizationSlug: string;
}) {
  const statuses = useMonitorStatuses(organizationId);
  const pathname = usePathname();
  const monitorsHref = `/${organizationSlug}/monitors`;

  return (
    <SidebarGroup className="group-data-[collapsible=icon]:hidden">
      <SidebarGroupLabel>
        Monitors
        {statuses.data ? (
          <span className="ms-1 text-sidebar-foreground/48 tabular-nums">
            ({statuses.data.length})
          </span>
        ) : null}
      </SidebarGroupLabel>
      <SidebarGroupAction
        render={<Link href={`${monitorsHref}/create`} aria-label="New monitor" />}
      >
        <PlusIcon />
      </SidebarGroupAction>
      <SidebarGroupContent>
        <SidebarMenu>
          {statuses.isPending ? (
            Array.from({ length: 3 }, (_, index) => (
              <SidebarMenuItem key={index}>
                <SidebarMenuSkeleton />
              </SidebarMenuItem>
            ))
          ) : statuses.isError ? (
            <p className="px-2 text-sidebar-foreground/64 text-xs">
              {statuses.error.message}
            </p>
          ) : statuses.data.length === 0 ? (
            <p className="px-2 text-sidebar-foreground/64 text-xs">
              No monitors yet.
            </p>
          ) : (
            statuses.data.map((monitor) => {
              const href = `${monitorsHref}/${monitor.id}`;
              return (
                <SidebarMenuItem key={monitor.id}>
                  <SidebarMenuButton
                    isActive={pathname === href || pathname.startsWith(`${href}/`)}
                    render={<Link href={href} />}
                  >
                    <StatusDot status={monitor.status} className="mx-1" />
                    <span className="truncate">{monitor.name}</span>
                  </SidebarMenuButton>
                  <MonitorActions
                    organizationId={organizationId}
                    monitor={monitor}
                    href={href}
                  />
                </SidebarMenuItem>
              );
            })
          )}
        </SidebarMenu>
      </SidebarGroupContent>
    </SidebarGroup>
  );
}

function MonitorActions({
  organizationId,
  monitor,
  href,
}: {
  organizationId: string;
  monitor: MonitorStatusSummary;
  href: string;
}) {
  const setPaused = useSetMonitorPaused({
    organizationId,
    monitorId: monitor.id,
  });
  const paused = monitor.status === "Paused";

  return (
    <Menu>
      <MenuTrigger
        render={
          <SidebarMenuAction
            showOnHover
            className="data-popup-open:opacity-100"
            aria-label={`Actions for ${monitor.name}`}
          />
        }
      >
        <EllipsisIcon />
      </MenuTrigger>
      <MenuPopup side="right" align="start">
        <MenuLinkItem render={<Link href={href} />}>
          <ExternalLinkIcon />
          Open
        </MenuLinkItem>
        <MenuItem
          disabled={setPaused.isPending}
          onClick={() => setPaused.mutate(!paused)}
        >
          {paused ? <PlayIcon /> : <PauseIcon />}
          {paused ? "Resume" : "Pause"}
        </MenuItem>
      </MenuPopup>
    </Menu>
  );
}
