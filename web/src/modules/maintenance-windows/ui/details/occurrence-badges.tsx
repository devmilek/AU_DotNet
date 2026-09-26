import { PencilLineIcon } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import {
  isRescheduled,
  occurrencePhase,
  type WindowOccurrence,
} from "../../lib/occurrence";

export function OccurrenceBadges({
  occurrence,
  now,
}: {
  occurrence: WindowOccurrence;
  now: Date;
}) {
  const phase = occurrencePhase(occurrence, now);

  return (
    <div className="flex flex-wrap gap-1">
      {phase === "inProgress" ? <Badge variant="info">In progress</Badge> : null}
      {phase === "ended" ? <Badge variant="secondary">Ended</Badge> : null}
      {phase === "cancelled" ? <Badge variant="secondary">Cancelled</Badge> : null}
      {isRescheduled(occurrence) ? (
        <Badge variant="outline">Rescheduled</Badge>
      ) : null}
      {occurrence.differsFromWindow ? (
        <Badge variant="outline">
          <PencilLineIcon />
          Edited
        </Badge>
      ) : null}
    </div>
  );
}
