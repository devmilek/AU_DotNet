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
import { type IncidentListParams, type IncidentRef, incidentKeys } from "./keys";

function toastError(title: string) {
  return (error: Error) =>
    toastManager.add({
      type: "error",
      title,
      description:
        error instanceof ApiError && error.status === 403 && !error.problem?.detail
          ? "You need admin permissions in this organization."
          : error.message,
    });
}

const incidentPath = ({ organizationId, incidentId }: IncidentRef) => ({
  path: { orgId: organizationId, id: incidentId },
});

export function useIncidents(organizationId: string, params: IncidentListParams) {
  const { status, page, pageSize = 20, monitorId } = params;

  return useQuery({
    queryKey: incidentKeys.list(organizationId, params),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/incidents", {
          params: {
            path: { orgId: organizationId },
            query: { status, page, pageSize, monitorId },
          },
        }),
        "Could not load incidents.",
      ),
    placeholderData: keepPreviousData,
    refetchInterval: 30_000,
  });
}

export function useIncident(ref: IncidentRef) {
  return useQuery({
    queryKey: incidentKeys.detail(ref.organizationId, ref.incidentId),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/incidents/{id}", { params: incidentPath(ref) }),
        "Could not load the incident.",
      ),
    refetchInterval: 30_000,
  });
}

export function useAcknowledgeIncident(ref: IncidentRef) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () =>
      ensureOk(
        api.POST("/api/{orgId}/incidents/{id}/acknowledge", {
          params: incidentPath(ref),
        }),
        "Could not acknowledge the incident.",
      ),
    onSuccess: () => {
      toastManager.add({ type: "success", title: "Incident acknowledged" });
    },
    onError: toastError("Could not acknowledge the incident"),
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: incidentKeys.organization(ref.organizationId),
      }),
  });
}

export function useUpdateIncident(ref: IncidentRef) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: { name: string | null; cause: string | null }) =>
      ensureOk(
        api.PUT("/api/{orgId}/incidents/{id}", {
          params: incidentPath(ref),
          body,
        }),
        "Could not save the incident.",
      ),
    onSuccess: () => {
      toastManager.add({ type: "success", title: "Incident updated" });
    },
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: incidentKeys.organization(ref.organizationId),
      }),
  });
}

export function useDeleteIncident(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (incidentId: string) =>
      ensureOk(
        api.DELETE("/api/{orgId}/incidents/{id}", {
          params: incidentPath({ organizationId, incidentId }),
        }),
        "Could not delete the incident.",
      ),
    onSuccess: () => {
      toastManager.add({ type: "success", title: "Incident deleted" });
    },
    onError: toastError("Could not delete the incident"),
    onSettled: (_, __, incidentId) =>
      queryClient.invalidateQueries({
        queryKey: incidentKeys.organization(organizationId),
        predicate: (query) =>
          !query.queryKey
            .join("/")
            .startsWith(incidentKeys.detail(organizationId, incidentId).join("/")),
      }),
  });
}
