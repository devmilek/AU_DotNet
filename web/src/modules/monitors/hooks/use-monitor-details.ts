"use client";

import {
  keepPreviousData,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { unwrap } from "@/lib/api/errors";
import type { components } from "@/lib/api/schema";
import type { ResponseTimeRange } from "@/modules/monitors/lib/response-time-range";
import { type MonitorRef, monitorKeys } from "./keys";

const path = ({ organizationId, monitorId }: MonitorRef) => ({
  path: { orgId: organizationId, monitorId },
});

export function useMonitor({ organizationId, monitorId }: MonitorRef) {
  return useQuery({
    queryKey: monitorKeys.detail(organizationId, monitorId),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/monitors/{id}", {
          params: { path: { orgId: organizationId, id: monitorId } },
        }),
        "Nie udało się pobrać monitora.",
      ),
  });
}

type MonitorStatus = components["schemas"]["MonitorStatusResponse"];
type MonitorStatusSummary = components["schemas"]["MonitorStatusSummaryResponse"];

const FALLBACK_INTERVAL_SECONDS = 30;
const CHECK_SETTLE_MS = 3_000;
const MIN_REFETCH_DELAY_MS = 1_000;

function nextStatusRefetchDelay(
  status: MonitorStatus | undefined,
  intervalSeconds: number,
  now = Date.now(),
): number | false {
  if (status?.status === "Paused") return false;

  const intervalMs = intervalSeconds * 1000;
  const checkedAt = status?.lastCheck?.checkedAt;
  if (!checkedAt) return intervalMs;

  const untilNextCheck =
    Date.parse(checkedAt) + intervalMs + CHECK_SETTLE_MS - now;

  return untilNextCheck > MIN_REFETCH_DELAY_MS
    ? Math.min(untilNextCheck, intervalMs + CHECK_SETTLE_MS)
    : intervalMs;
}

export function useMonitorStatus(ref: MonitorRef, intervalSeconds?: number) {
  const queryClient = useQueryClient();

  return useQuery({
    queryKey: monitorKeys.status(ref.organizationId, ref.monitorId),
    queryFn: async () => {
      const previous = queryClient.getQueryData<MonitorStatus>(
        monitorKeys.status(ref.organizationId, ref.monitorId),
      );
      const status = await unwrap(
        api.GET("/api/{orgId}/monitors/{monitorId}/status", {
          params: path(ref),
        }),
        "Nie udało się pobrać statusu monitora.",
      );

      if (
        previous &&
        previous.lastCheck?.checkedAt !== status.lastCheck?.checkedAt
      ) {
        void queryClient.invalidateQueries({
          queryKey: monitorKeys.uptime(ref.organizationId, ref.monitorId),
        });
      }

      queryClient.setQueryData<MonitorStatusSummary[]>(
        monitorKeys.statuses(ref.organizationId),
        (summaries) =>
          summaries?.map((summary) =>
            summary.id === ref.monitorId
              ? { ...summary, status: status.status }
              : summary,
          ),
      );

      return status;
    },
    refetchInterval: (query) =>
      nextStatusRefetchDelay(
        query.state.data,
        intervalSeconds ?? FALLBACK_INTERVAL_SECONDS,
      ),
  });
}

export function useMonitorUptime(ref: MonitorRef) {
  return useQuery({
    queryKey: monitorKeys.uptime(ref.organizationId, ref.monitorId),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/monitors/{monitorId}/uptime", {
          params: path(ref),
        }),
        "Nie udało się pobrać uptime monitora.",
      ),
  });
}

export function useMonitorResponseTimes(
  ref: MonitorRef,
  range: ResponseTimeRange,
) {
  return useQuery({
    queryKey: monitorKeys.responseTimes(
      ref.organizationId,
      ref.monitorId,
      range,
    ),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/monitors/{monitorId}/response-times", {
          params: { ...path(ref), query: { range } },
        }),
        "Nie udało się pobrać czasów odpowiedzi.",
      ),
    refetchInterval: 60_000,
    // przy zmianie zakresu zostaje poprzedni wykres zamiast skeletona
    placeholderData: keepPreviousData,
  });
}
