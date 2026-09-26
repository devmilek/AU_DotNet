"use client";

import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { unwrap } from "@/lib/api/errors";
import { monitorKeys } from "./keys";

const REFRESH_INTERVAL_MS = 30_000;

export function useMonitorStatuses(organizationId: string) {
  return useQuery({
    queryKey: monitorKeys.statuses(organizationId),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/monitors/statuses", {
          params: { path: { orgId: organizationId } },
        }),
        "Nie udało się pobrać statusów monitorów.",
      ),
    refetchInterval: REFRESH_INTERVAL_MS,
  });
}
