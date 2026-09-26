"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api/client";
import { monitorKeys } from "@/modules/monitors/hooks/keys";
import {
  createHttpMonitorDefaults,
  toCreateMonitorRequest,
} from "@/modules/monitors/schemas/create-http-monitor";
import { HttpMonitorForm, toServerErrors } from "./http-monitor-form";

export function CreateHttpMonitorForm({
  organizationId,
  organizationSlug,
}: {
  organizationId: string;
  organizationSlug: string;
}) {
  const router = useRouter();
  const queryClient = useQueryClient();
  const monitorsHref = `/${organizationSlug}/monitors`;

  return (
    <HttpMonitorForm
      initialValues={createHttpMonitorDefaults}
      submitLabel="Create monitor"
      cancelHref={monitorsHref}
      onSubmit={async (values) => {
        const { response, error } = await api.POST("/api/{orgId}/monitors", {
          params: { path: { orgId: organizationId } },
          body: toCreateMonitorRequest(values),
        });

        if (!response.ok) {
          throw toServerErrors(error, "Could not create the monitor.");
        }

        await queryClient.invalidateQueries({
          queryKey: monitorKeys.lists(organizationId),
        });
        router.push(monitorsHref);
      }}
    />
  );
}
