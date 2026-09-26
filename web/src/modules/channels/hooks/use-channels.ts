"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toastManager } from "@/components/ui/toast";
import { api } from "@/lib/api/client";
import { ApiError, ensureOk, unwrap } from "@/lib/api/errors";
import type { components } from "@/lib/api/schema";
import type { MonitorRef } from "@/modules/monitors/hooks/keys";
import { type ChannelRef, channelKeys } from "./keys";

export function useChannels(organizationId: string) {
  return useQuery({
    queryKey: channelKeys.list(organizationId),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/channels", {
          params: { path: { orgId: organizationId } },
        }),
        "Nie udało się pobrać kanałów powiadomień.",
      ),
  });
}

export function useChannel({ organizationId, channelId }: ChannelRef) {
  return useQuery({
    queryKey: channelKeys.detail(organizationId, channelId),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/channels/{id}", {
          params: { path: { orgId: organizationId, id: channelId } },
        }),
        "Nie udało się pobrać kanału powiadomień.",
      ),
  });
}

export function useCreateChannel(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (
      body: components["schemas"]["CreateNotificationChannelRequest"],
    ) =>
      unwrap(
        api.POST("/api/{orgId}/channels", {
          params: { path: { orgId: organizationId } },
          body,
        }),
        "Nie udało się utworzyć kanału powiadomień.",
      ),
    onSuccess: () =>
      queryClient.invalidateQueries({
        queryKey: channelKeys.organization(organizationId),
      }),
  });
}

export function useSetChannelMonitors(ref: ChannelRef) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (monitorIds: string[]) =>
      ensureOk(
        api.PUT("/api/{orgId}/channels/{id}/monitors", {
          params: { path: { orgId: ref.organizationId, id: ref.channelId } },
          body: { monitorIds },
        }),
        "Nie udało się zapisać monitorów kanału.",
      ),
    onSuccess: () => {
      toastManager.add({ type: "success", title: "Monitors updated" });
    },
    onError: (error) => {
      toastManager.add({
        type: "error",
        title: "Could not update monitors",
        description:
          error instanceof ApiError && error.status === 403
            ? "You need admin permissions in this organization."
            : error.message,
      });
    },
    // lista pokazuje liczbę monitorów, szczegóły — ich listę
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: channelKeys.organization(ref.organizationId),
      }),
  });
}

export function useMonitorChannels({ organizationId, monitorId }: MonitorRef) {
  return useQuery({
    queryKey: channelKeys.forMonitor(organizationId, monitorId),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/monitors/{monitorId}/channels", {
          params: { path: { orgId: organizationId, monitorId } },
        }),
        "Could not load the monitor's notification channels.",
      ),
  });
}

export function useToggleMonitorChannel(ref: MonitorRef) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ channelId, assign }: { channelId: string; assign: boolean }) => {
      const params = {
        path: {
          orgId: ref.organizationId,
          monitorId: ref.monitorId,
          channelId,
        },
      };

      return assign
        ? ensureOk(
            api.POST("/api/{orgId}/monitors/{monitorId}/channels/{channelId}", {
              params,
            }),
            "Could not connect the channel.",
          )
        : ensureOk(
            api.DELETE("/api/{orgId}/monitors/{monitorId}/channels/{channelId}", {
              params,
            }),
            "Could not disconnect the channel.",
          );
    },
    onError: (error, { assign }) => {
      toastManager.add({
        type: "error",
        title: assign
          ? "Could not connect the channel"
          : "Could not disconnect the channel",
        description:
          error instanceof ApiError && error.status === 403
            ? "You need admin permissions in this organization."
            : error.message,
      });
    },
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: channelKeys.organization(ref.organizationId),
      }),
  });
}
