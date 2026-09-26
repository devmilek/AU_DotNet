"use client";

import { BellRingIcon, PauseIcon, PencilIcon, PlayIcon } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Group, GroupSeparator } from "@/components/ui/group";
import type { components } from "@/lib/api/schema";
import type { MonitorRef } from "@/modules/monitors/hooks/keys";
import {
  useSendTestAlert,
  useSetMonitorPaused,
} from "@/modules/monitors/hooks/use-monitor-actions";
import { monitorTypeLabel } from "@/modules/monitors/lib/search-params";

type Monitor = components["schemas"]["MonitorResponse"];

export function MonitorHeader({
  monitor,
  monitorRef,
  editHref,
}: {
  monitor: Monitor;
  monitorRef: MonitorRef;
  editHref: string;
}) {
  const setPaused = useSetMonitorPaused(monitorRef);
  const sendTestAlert = useSendTestAlert(monitorRef);

  return (
    <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
      <div className="min-w-0 space-y-1">
        <h1 className="truncate font-heading text-2xl">{monitor.name}</h1>
        <p className="truncate text-muted-foreground text-sm">
          <span className="text-foreground">
            {monitorTypeLabel(monitor.type)}
          </span>{" "}
          monitor for{" "}
          <a
            className="text-foreground underline underline-offset-4 hover:text-foreground/80"
            href={monitor.target}
            rel="noopener noreferrer"
            target="_blank"
          >
            {monitor.target}
          </a>
        </p>
      </div>

      <Group aria-label="Monitor actions" className="shrink-0">
        <Button
          variant="outline"
          loading={sendTestAlert.isPending}
          onClick={() => sendTestAlert.mutate()}
        >
          <BellRingIcon />
          Test alert
        </Button>
        <GroupSeparator />
        <Button
          variant="outline"
          loading={setPaused.isPending}
          onClick={() => setPaused.mutate(monitor.isActive)}
        >
          {monitor.isActive ? <PauseIcon /> : <PlayIcon />}
          {monitor.isActive ? "Pause monitor" : "Resume monitor"}
        </Button>
        <GroupSeparator />
        <Button variant="outline" render={<Link href={editHref} />}>
          <PencilIcon />
          Edit
        </Button>
      </Group>
    </div>
  );
}
