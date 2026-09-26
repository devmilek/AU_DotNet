"use client";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { cn } from "@/lib/utils";
import { logoInitials } from "../lib/logo";

type OrganizationAvatarProps = {
  name: string;
  logoUrl?: string | null;
  className?: string;
  fallbackClassName?: string;
};

export function OrganizationAvatar({
  name,
  logoUrl,
  className,
  fallbackClassName,
}: OrganizationAvatarProps) {
  return (
    <Avatar className={cn("rounded-lg", className)}>
      {logoUrl ? <AvatarImage src={logoUrl} alt="" /> : null}
      <AvatarFallback className={cn("rounded-[inherit]", fallbackClassName)}>
        {logoInitials(name) ?? "?"}
      </AvatarFallback>
    </Avatar>
  );
}
