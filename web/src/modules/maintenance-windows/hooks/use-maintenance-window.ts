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
import type { MaintenanceWindowChanges } from "../schemas/create-maintenance-window";
import { type MaintenanceWindowRef, maintenanceWindowKeys } from "./keys";

const windowPath = ({ organizationId, windowId }: MaintenanceWindowRef) => ({
  path: { orgId: organizationId, id: windowId },
});

function errorDescription(error: Error) {
  return error instanceof ApiError && error.status === 403
    ? "You need admin permissions in this organization."
    : error.message;
}

export function useMaintenanceWindow(ref: MaintenanceWindowRef) {
  return useQuery({
    queryKey: maintenanceWindowKeys.detail(ref.organizationId, ref.windowId),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/maintenance-windows/{id}", {
          params: windowPath(ref),
        }),
        "Nie udało się pobrać okna serwisowego.",
      ),
  });
}

export function useWindowOccurrences(
  ref: MaintenanceWindowRef,
  range: { from: Date; to: Date },
) {
  const from = range.from.toISOString();
  const to = range.to.toISOString();

  return useQuery({
    queryKey: maintenanceWindowKeys.windowOccurrences(
      ref.organizationId,
      ref.windowId,
      from,
      to,
    ),
    queryFn: () =>
      unwrap(
        api.GET("/api/{orgId}/maintenance-windows/{id}/occurrences", {
          params: { ...windowPath(ref), query: { from, to } },
        }),
        "Nie udało się pobrać wystąpień okna serwisowego.",
      ),
    placeholderData: keepPreviousData,
  });
}

export function useUpdateMaintenanceWindow(ref: MaintenanceWindowRef) {
  const queryClient = useQueryClient();
  const params = windowPath(ref);

  return useMutation({
    mutationFn: async ({
      changes,
      applyToPastOccurrences,
    }: {
      changes: MaintenanceWindowChanges;
      applyToPastOccurrences: boolean;
    }) => {
      if (changes.content) {
        await ensureOk(
          api.PUT("/api/{orgId}/maintenance-windows/{id}/content", {
            params,
            body: { ...changes.content, applyToPastOccurrences },
          }),
          "Nie udało się zapisać nazwy i opisu.",
        );
      }

      if (changes.schedule) {
        await ensureOk(
          api.PUT("/api/{orgId}/maintenance-windows/{id}/schedule", {
            params,
            body: changes.schedule,
          }),
          "Nie udało się zapisać harmonogramu.",
        );
      }

      if (changes.policy) {
        await ensureOk(
          api.PUT("/api/{orgId}/maintenance-windows/{id}/policy", {
            params,
            body: changes.policy,
          }),
          "Nie udało się zapisać ustawień.",
        );
      }

      if (changes.monitorIds) {
        await ensureOk(
          api.PUT("/api/{orgId}/maintenance-windows/{id}/monitors", {
            params,
            body: { monitorIds: changes.monitorIds },
          }),
          "Nie udało się zapisać monitorów.",
        );
      }
    },
    onSuccess: async () => {
      toastManager.add({ type: "success", title: "Changes saved" });
      await queryClient.invalidateQueries({
        queryKey: maintenanceWindowKeys.organization(ref.organizationId),
      });
    },
  });
}

function useOccurrenceMutation<TVariables>(
  ref: MaintenanceWindowRef,
  request: (variables: TVariables) => Promise<void>,
  messages: { success: string; error: string },
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: request,
    onSuccess: () => {
      toastManager.add({ type: "success", title: messages.success });
    },
    onError: (error) => {
      toastManager.add({
        type: "error",
        title: messages.error,
        description: errorDescription(error),
      });
    },
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: maintenanceWindowKeys.organization(ref.organizationId),
      }),
  });
}

export function useUpdateOccurrenceContent(ref: MaintenanceWindowRef) {
  return useOccurrenceMutation(
    ref,
    ({
      occurrenceId,
      name,
      description,
    }: {
      occurrenceId: string;
      name: string;
      description: string | null;
    }) =>
      ensureOk(
        api.PUT(
          "/api/{orgId}/maintenance-windows/{id}/occurrences/{occurrenceId}/content",
          {
            params: {
              path: { ...windowPath(ref).path, occurrenceId },
            },
            body: { name, description },
          },
        ),
        "Nie udało się zapisać wystąpienia.",
      ),
    { success: "Occurrence updated", error: "Could not update the occurrence" },
  );
}

export function useResetOccurrenceContent(ref: MaintenanceWindowRef) {
  return useOccurrenceMutation(
    ref,
    (occurrenceId: string) =>
      ensureOk(
        api.DELETE(
          "/api/{orgId}/maintenance-windows/{id}/occurrences/{occurrenceId}/content",
          {
            params: {
              path: { ...windowPath(ref).path, occurrenceId },
            },
          },
        ),
        "Nie udało się przywrócić treści wystąpienia.",
      ),
    {
      success: "Occurrence follows the window again",
      error: "Could not restore the occurrence",
    },
  );
}

export function useCancelOccurrence(ref: MaintenanceWindowRef) {
  return useOccurrenceMutation(
    ref,
    (occurrenceId: string) =>
      ensureOk(
        api.POST(
          "/api/{orgId}/maintenance-windows/{id}/occurrences/{occurrenceId}/cancel",
          { params: { path: { ...windowPath(ref).path, occurrenceId } } },
        ),
        "Nie udało się anulować wystąpienia.",
      ),
    { success: "Occurrence cancelled", error: "Could not cancel the occurrence" },
  );
}

export function useRestoreOccurrence(ref: MaintenanceWindowRef) {
  return useOccurrenceMutation(
    ref,
    (occurrenceId: string) =>
      ensureOk(
        api.POST(
          "/api/{orgId}/maintenance-windows/{id}/occurrences/{occurrenceId}/restore",
          { params: { path: { ...windowPath(ref).path, occurrenceId } } },
        ),
        "Nie udało się przywrócić wystąpienia.",
      ),
    { success: "Occurrence restored", error: "Could not restore the occurrence" },
  );
}
