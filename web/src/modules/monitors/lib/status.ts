import type { components } from "@/lib/api/schema";

export type MonitorStatus = components["schemas"]["MonitorStatus"];

export const monitorStatusAppearance: Record<
  MonitorStatus,
  { label: string; dotClassName: string; pulseClassName: string | null }
> = {
  Up: {
    label: "Up",
    dotClassName: "bg-success",
    pulseClassName: "bg-success [animation-duration:2s]",
  },
  Down: {
    label: "Down",
    dotClassName: "bg-destructive",
    pulseClassName: "bg-destructive",
  },
  Paused: {
    label: "Paused",
    dotClassName: "bg-warning",
    pulseClassName: null,
  },
  Pending: {
    label: "Pending",
    dotClassName: "bg-muted-foreground/64",
    pulseClassName: null,
  },
};
