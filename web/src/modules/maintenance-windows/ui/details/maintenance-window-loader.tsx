"use client";

import { ArrowLeftIcon, CircleAlertIcon, SearchXIcon } from "lucide-react";
import Link from "next/link";
import type React from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { useIsClient } from "@/hooks/use-is-client";
import { ApiError } from "@/lib/api/errors";
import type { components } from "@/lib/api/schema";
import type { MaintenanceWindowRef } from "../../hooks/keys";
import { useMaintenanceWindow } from "../../hooks/use-maintenance-window";

type MaintenanceWindow = components["schemas"]["MaintenanceWindowResponse"];

export function BackLink({ href, label }: { href: string; label: string }) {
  return (
    <Button
      size="sm"
      variant="ghost"
      className="-ms-2 self-start"
      render={<Link href={href} />}
    >
      <ArrowLeftIcon />
      {label}
    </Button>
  );
}

export function MaintenanceWindowLoader({
  windowRef,
  backLink,
  fallback,
  children,
}: {
  windowRef: MaintenanceWindowRef;
  backLink: React.ReactNode;
  fallback: React.ReactNode;
  children: (maintenanceWindow: MaintenanceWindow) => React.ReactNode;
}) {
  const maintenanceWindow = useMaintenanceWindow(windowRef);
  const isClient = useIsClient();

  if (maintenanceWindow.isError) {
    const notFound =
      maintenanceWindow.error instanceof ApiError &&
      maintenanceWindow.error.status === 404;

    return (
      <div className="flex flex-col gap-6">
        {backLink}
        {notFound ? (
          <Empty>
            <EmptyHeader>
              <EmptyMedia variant="icon">
                <SearchXIcon />
              </EmptyMedia>
              <EmptyTitle>Maintenance window not found</EmptyTitle>
              <EmptyDescription>
                It may have been deleted or belongs to another organization.
              </EmptyDescription>
            </EmptyHeader>
          </Empty>
        ) : (
          <Alert variant="error">
            <CircleAlertIcon />
            <AlertDescription>{maintenanceWindow.error.message}</AlertDescription>
          </Alert>
        )}
      </div>
    );
  }

  if (!maintenanceWindow.data || !isClient) {
    return (
      <div className="flex flex-col gap-6">
        {backLink}
        {fallback}
      </div>
    );
  }

  return children(maintenanceWindow.data);
}
