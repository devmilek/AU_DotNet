"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toastManager } from "@/components/ui/toast";
import { api } from "@/lib/api/client";
import { ApiError, ensureOk, unwrap } from "@/lib/api/errors";
import type { OrganizationRole } from "../lib/roles";
import { organizationKeys } from "./keys";

function errorDescription(error: Error) {
  return error instanceof ApiError && error.status === 403 && !error.problem?.detail
    ? "You don’t have permission to do this."
    : error.message;
}

function toastError(title: string) {
  return (error: Error) =>
    toastManager.add({ type: "error", title, description: errorDescription(error) });
}

const orgPath = (organizationId: string) => ({
  path: { orgId: organizationId },
});

export function useOrganization(organizationId: string) {
  return useQuery({
    queryKey: organizationKeys.detail(organizationId),
    queryFn: () =>
      unwrap(
        api.GET("/api/organizations/{orgId}", { params: orgPath(organizationId) }),
        "Nie udało się pobrać organizacji.",
      ),
  });
}

export function useUpdateOrganization(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (name: string) =>
      ensureOk(
        api.PUT("/api/organizations/{orgId}", {
          params: orgPath(organizationId),
          body: { name },
        }),
        "Nie udało się zapisać organizacji.",
      ),
    onSuccess: () => {
      toastManager.add({ type: "success", title: "Organization updated" });
    },
    onSettled: () =>
      queryClient.invalidateQueries({ queryKey: organizationKeys.all }),
  });
}

export function useDeleteOrganization(organizationId: string) {
  return useMutation({
    mutationFn: (confirmationName: string) =>
      ensureOk(
        api.DELETE("/api/organizations/{orgId}", {
          params: orgPath(organizationId),
          body: { confirmationName },
        }),
        "Nie udało się usunąć organizacji.",
      ),
  });
}

export function useMembers(organizationId: string) {
  return useQuery({
    queryKey: organizationKeys.members(organizationId),
    queryFn: () =>
      unwrap(
        api.GET("/api/organizations/{orgId}/members", {
          params: orgPath(organizationId),
        }),
        "Nie udało się pobrać członków organizacji.",
      ),
  });
}

export function useChangeMemberRole(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: OrganizationRole }) =>
      ensureOk(
        api.PUT("/api/organizations/{orgId}/members/{userId}/role", {
          params: { path: { orgId: organizationId, userId } },
          body: { role },
        }),
        "Nie udało się zmienić roli.",
      ),
    onSuccess: () => {
      toastManager.add({ type: "success", title: "Role updated" });
    },
    onError: toastError("Could not change the role"),
    onSettled: () =>
      queryClient.invalidateQueries({ queryKey: organizationKeys.all }),
  });
}

export function useRemoveMember(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: string) =>
      ensureOk(
        api.DELETE("/api/organizations/{orgId}/members/{userId}", {
          params: { path: { orgId: organizationId, userId } },
        }),
        "Nie udało się usunąć członka.",
      ),
    onError: toastError("Could not remove the member"),
    onSettled: () =>
      queryClient.invalidateQueries({ queryKey: organizationKeys.all }),
  });
}

export function useInvitations(organizationId: string, enabled: boolean) {
  return useQuery({
    queryKey: organizationKeys.invitations(organizationId),
    queryFn: () =>
      unwrap(
        api.GET("/api/organizations/{orgId}/invitations", {
          params: orgPath(organizationId),
        }),
        "Nie udało się pobrać zaproszeń.",
      ),
    enabled,
  });
}

export function useInviteMember(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: { email: string; role: OrganizationRole }) =>
      unwrap(
        api.POST("/api/organizations/{orgId}/invitations", {
          params: orgPath(organizationId),
          body,
        }),
        "Nie udało się wysłać zaproszenia.",
      ),
    onSuccess: (invitation) => {
      toastManager.add({
        type: "success",
        title: "Invitation sent",
        description: `We emailed ${invitation.email}.`,
      });
    },
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: organizationKeys.invitations(organizationId),
      }),
  });
}

export function useRevokeInvitation(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (invitationId: string) =>
      ensureOk(
        api.DELETE("/api/organizations/{orgId}/invitations/{invitationId}", {
          params: { path: { orgId: organizationId, invitationId } },
        }),
        "Nie udało się odwołać zaproszenia.",
      ),
    onSuccess: () => {
      toastManager.add({ type: "success", title: "Invitation revoked" });
    },
    onError: toastError("Could not revoke the invitation"),
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: organizationKeys.invitations(organizationId),
      }),
  });
}

export function useInvitationPreview(token: string) {
  return useQuery({
    queryKey: organizationKeys.invitation(token),
    queryFn: () =>
      unwrap(
        api.GET("/api/invitations/{token}", { params: { path: { token } } }),
        "Nie udało się pobrać zaproszenia.",
      ),
    enabled: token.length > 0,
    retry: false,
  });
}

export function useAcceptInvitation(token: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () =>
      unwrap(
        api.POST("/api/invitations/{token}/accept", {
          params: { path: { token } },
        }),
        "Nie udało się przyjąć zaproszenia.",
      ),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: organizationKeys.all }),
  });
}
