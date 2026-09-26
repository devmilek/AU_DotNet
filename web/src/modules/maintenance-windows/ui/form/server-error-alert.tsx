import { CircleAlertIcon } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import type { ServerError } from "../../lib/server-error";

export function ServerErrorAlert({ error }: { error: ServerError | null }) {
  if (!error) return null;

  return (
    <Alert
      key={`${error.title}${error.details.join()}`}
      ref={(node) => node?.scrollIntoView({ block: "center" })}
      variant="error"
    >
      <CircleAlertIcon />
      <AlertTitle>{error.title}</AlertTitle>
      {error.details.length > 0 ? (
        <AlertDescription>
          <ul className="list-disc ps-4">
            {error.details.map((detail) => (
              <li key={detail}>{detail}</li>
            ))}
          </ul>
        </AlertDescription>
      ) : null}
    </Alert>
  );
}
