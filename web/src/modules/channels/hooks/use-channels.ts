"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toastManager } from "@/components/ui/toast";
import { api } from "@/lib/api/client";
import { ApiError, ensureOk, unwrap } from "@/lib/api/errors";
import type { components } from "@/lib/api/schema";
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
