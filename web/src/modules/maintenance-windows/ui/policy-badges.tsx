import { BellOffIcon, GaugeIcon } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

export function PolicyBadges({
  suppressNotifications,
  excludeFromSla,
  className,
}: {
  suppressNotifications: boolean;
  excludeFromSla: boolean;
  className?: string;
}) {
  if (!suppressNotifications && !excludeFromSla) return null;

  return (
    <div className={cn("flex flex-wrap gap-1", className)}>
      {suppressNotifications ? (
        <Badge variant="outline">
          <BellOffIcon />
          Alerts muted
        </Badge>
      ) : null}
      {excludeFromSla ? (
        <Badge variant="outline">
          <GaugeIcon />
          Excluded from SLA
        </Badge>
      ) : null}
    </div>
  );
}
