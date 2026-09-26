"use client";

import { ArrowLeftIcon, CircleAlertIcon, InfoIcon } from "lucide-react";
import Link from "next/link";
import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { toastManager } from "@/components/ui/toast";
import { api } from "@/lib/api/client";
import { type MonitorRef, monitorKeys } from "@/modules/monitors/hooks/keys";
import { useMonitor } from "@/modules/monitors/hooks/use-monitor-details";
import {
  storedSecretsOf,
  toUpdateMonitorRequest,
  valuesFromMonitor,
} from "@/modules/monitors/schemas/create-http-monitor";
import { HttpMonitorForm, toServerErrors } from "./http-monitor-form";

export function EditHttpMonitorView({
  monitorRef,
  monitorHref,
}: {
  monitorRef: MonitorRef;
  monitorHref: string;
}) {
  const router = useRouter();
  const queryClient = useQueryClient();
  const monitor = useMonitor(monitorRef);

  const header = (
    <div className="flex flex-col items-start gap-3">
      <Button
        size="sm"
        variant="ghost"
        className="-ms-2"
        render={<Link href={monitorHref} />}
      >
        <ArrowLeftIcon />
        {monitor.data?.name ?? "Monitor"}
      </Button>
      <div className="space-y-1">
        <h1 className="font-heading text-2xl">Edit monitor</h1>
        <p className="text-muted-foreground text-sm">
          Changes apply from the next check.
        </p>
      </div>
    </div>
  );

  if (monitor.isError) {
    return (
      <>
        {header}
        <Alert variant="error">
          <CircleAlertIcon />
          <AlertDescription>{monitor.error.message}</AlertDescription>
        </Alert>
      </>
    );
  }

  if (!monitor.data) {
    return (
      <>
        {header}
        <div className="flex flex-col gap-6">
          <Skeleton className="h-48 w-full rounded-xl" />
          <Skeleton className="h-56 w-full rounded-xl" />
          <Skeleton className="h-48 w-full rounded-xl" />
        </div>
      </>
    );
  }

  if (monitor.data.type !== "Http") {
    return (
      <>
        {header}
        <Alert variant="info">
          <InfoIcon />
          <AlertDescription>
            Only HTTP monitors can be edited here for now.
          </AlertDescription>
        </Alert>
      </>
    );
  }

  return (
    <>
      {header}
      <HttpMonitorForm
        key={monitor.data.updatedAt}
        initialValues={valuesFromMonitor(monitor.data)}
        storedSecrets={storedSecretsOf(monitor.data)}
        submitLabel="Save changes"
        cancelHref={monitorHref}
        onSubmit={async (values) => {
          const { response, error } = await api.PUT(
            "/api/{orgId}/monitors/{monitorId}",
            {
              params: {
                path: {
                  orgId: monitorRef.organizationId,
                  monitorId: monitorRef.monitorId,
                },
              },
              body: toUpdateMonitorRequest(values),
            },
          );

          if (!response.ok) {
            throw toServerErrors(error, "Could not save the monitor.");
          }

          await queryClient.invalidateQueries({
            queryKey: monitorKeys.organization(monitorRef.organizationId),
          });
          toastManager.add({ type: "success", title: "Monitor updated" });
          router.push(monitorHref);
        }}
      />
    </>
  );
}
