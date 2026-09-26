"use client";

import {
  keepPreviousData,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import { toastManager } from "@/components/ui/toast";
import { api } from "@/lib/api/client";
import { ApiError, ensureOk, unwrap } from "@/lib/api/errors";
import type { components } from "@/lib/api/schema";
import { maintenanceWindowKeys } from "./keys";

export function useMaintenanceWindows(organizationId: string) {
  return useQuery({
    queryKey: maintenanceWindowKeys.list(organizationId),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/maintenance-windows", {
          params: { path: { orgId: organizationId } },
        }),
        "Nie udało się pobrać okien serwisowych.",
      ),
  });
}

export function useMaintenanceOccurrences(
  organizationId: string,
  range: { from: Date; to: Date },
) {
  const from = range.from.toISOString();
  const to = range.to.toISOString();

  return useQuery({
    queryKey: maintenanceWindowKeys.occurrences(organizationId, from, to),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/maintenance-windows/occurrences", {
          params: { path: { orgId: organizationId }, query: { from, to } },
        }),
        "Nie udało się pobrać wystąpień okien serwisowych.",
      ),
    placeholderData: keepPreviousData,
  });
}

export function useCreateMaintenanceWindow(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (
      body: components["schemas"]["CreateMaintenanceWindowRequest"],
    ) =>
      unwrap(
        api.POST("/api/{orgId}/maintenance-windows", {
          params: { path: { orgId: organizationId } },
          body,
        }),
        "Nie udało się utworzyć okna serwisowego.",
      ),
    onSuccess: () =>
      queryClient.invalidateQueries({
        queryKey: maintenanceWindowKeys.organization(organizationId),
      }),
  });
}

export function useDeleteMaintenanceWindow(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (windowId: string) =>
      ensureOk(
        api.DELETE("/api/{orgId}/maintenance-windows/{id}", {
          params: { path: { orgId: organizationId, id: windowId } },
        }),
        "Nie udało się usunąć okna serwisowego.",
      ),
    onSuccess: () => {
      toastManager.add({ type: "success", title: "Maintenance window deleted" });
    },
    onError: (error) => {
      toastManager.add({
        type: "error",
        title: "Could not delete the maintenance window",
        description:
          error instanceof ApiError && error.status === 403
            ? "You need admin permissions in this organization."
            : error.message,
      });
    },
    onSettled: (_, __, windowId) =>
      queryClient.invalidateQueries({
        queryKey: maintenanceWindowKeys.organization(organizationId),
        predicate: (query) =>
          !query.queryKey.join("/").startsWith(
            maintenanceWindowKeys.detail(organizationId, windowId).join("/"),
          ),
      }),
  });
}
