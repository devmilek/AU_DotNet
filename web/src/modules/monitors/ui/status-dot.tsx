import { cn } from "@/lib/utils";
import { type MonitorStatus, monitorStatusAppearance } from "../lib/status";

export function StatusDot({
  status,
  className,
}: {
  status: MonitorStatus;
  className?: string;
}) {
  const appearance = monitorStatusAppearance[status];

  return (
    <span
      role="img"
      aria-label={appearance.label}
      className={cn("relative flex size-2 shrink-0", className)}
    >
      {appearance.pulseClassName ? (
        <span
          className={cn(
            "absolute inline-flex size-full animate-ping rounded-full opacity-64 motion-reduce:hidden",
            appearance.pulseClassName,
          )}
        />
      ) : null}
      <span
        className={cn(
          "relative inline-flex size-2 rounded-full",
          appearance.dotClassName,
        )}
      />
    </span>
  );
}
