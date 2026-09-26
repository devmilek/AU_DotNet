"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";

export function SettingsNav({ settingsHref }: { settingsHref: string }) {
  const pathname = usePathname();

  const items = [
    { href: settingsHref, label: "General" },
    { href: `${settingsHref}/members`, label: "Members" },
  ];

  return (
    <nav aria-label="Settings" className="flex gap-1 border-b">
      {items.map((item) => {
        const active = pathname === item.href;
        return (
          <Link
            key={item.href}
            href={item.href}
            aria-current={active ? "page" : undefined}
            className={cn(
              "-mb-px border-b-2 px-3 py-2 font-medium text-sm transition-colors",
              active
                ? "border-primary text-foreground"
                : "border-transparent text-muted-foreground hover:text-foreground",
            )}
          >
            {item.label}
          </Link>
        );
      })}
    </nav>
  );
}
