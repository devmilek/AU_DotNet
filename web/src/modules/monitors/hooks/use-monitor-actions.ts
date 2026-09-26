"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toastManager } from "@/components/ui/toast";
import { api } from "@/lib/api/client";
import { ApiError, ensureOk } from "@/lib/api/errors";
import { type MonitorRef, monitorKeys } from "./keys";

function errorDescription(error: Error) {
  if (error instanceof ApiError && error.status === 403) {
    return "You need admin permissions in this organization.";
  }
  return error.message;
}

const path = ({ organizationId, monitorId }: MonitorRef) => ({
  path: { orgId: organizationId, monitorId },
});

export function useSetMonitorPaused(ref: MonitorRef) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (paused: boolean) =>
      paused
        ? ensureOk(
            api.POST("/api/{orgId}/monitors/{monitorId}/pause", {
              params: path(ref),
            }),
            "Nie udało się wstrzymać monitora.",
          )
        : ensureOk(
            api.POST("/api/{orgId}/monitors/{monitorId}/resume", {
              params: path(ref),
            }),
            "Nie udało się wznowić monitora.",
          ),
    onSuccess: (_, paused) => {
      toastManager.add({
        type: "success",
        title: paused ? "Monitor paused" : "Monitor resumed",
        description: paused
          ? "Checks are stopped until you resume it."
          : "The next check will run in a moment.",
      });
    },
    onError: (error, paused) => {
      toastManager.add({
        type: "error",
        title: paused ? "Could not pause monitor" : "Could not resume monitor",
        description: errorDescription(error),
      });
    },
    // odświeża szczegóły (status, isActive) i listę, bo status monitora jest też tam widoczny
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: monitorKeys.organization(ref.organizationId),
      }),
  });
}

export function useSendTestAlert(ref: MonitorRef) {
  return useMutation({
    mutationFn: () =>
      ensureOk(
        api.POST("/api/{orgId}/monitors/{monitorId}/test-notification", {
          params: path(ref),
        }),
        "Nie udało się wysłać testowego alertu.",
      ),
    onSuccess: () => {
      toastManager.add({
        type: "success",
        title: "Test alert sent",
        description:
          "Check the notification channels assigned to this monitor.",
      });
    },
    onError: (error) => {
      toastManager.add({
        type: "error",
        title: "Could not send test alert",
        description: errorDescription(error),
      });
    },
  });
}
