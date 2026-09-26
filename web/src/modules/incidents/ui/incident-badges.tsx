import { CircleCheckIcon, CircleDotIcon, EyeIcon, EyeOffIcon } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import type { components } from "@/lib/api/schema";

type Incident = components["schemas"]["IncidentResponse"];

export function IncidentStatusBadge({ incident }: { incident: Incident }) {
  if (incident.status === "Resolved") {
    return (
      <Badge variant="success">
        <CircleCheckIcon />
        Resolved
      </Badge>
    );
  }

  return (
    <Badge variant="error">
      <CircleDotIcon className="animate-pulse" />
      Ongoing
    </Badge>
  );
}

export function AcknowledgementBadge({ incident }: { incident: Incident }) {
  const { acknowledgement } = incident;

  if (acknowledgement) {
    return (
      <Badge variant="info">
        <EyeIcon />
        {acknowledgement.userName
          ? `Acknowledged by ${acknowledgement.userName}`
          : "Acknowledged"}
      </Badge>
    );
  }

  if (incident.status === "Resolved") {
    return (
      <Badge variant="secondary">
        <EyeOffIcon />
        Not acknowledged
      </Badge>
    );
  }

  return (
    <Badge variant="warning">
      <EyeOffIcon />
      Not acknowledged yet
    </Badge>
  );
}
