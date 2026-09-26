import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import {
  type ChannelTypeOption,
  channelTypeLabels,
  channelTypeOptions,
} from "@/modules/channels/lib/channel-type";
import { ChannelTypeIcon } from "./channel-type-icon";

export function ChannelTypePicker({
  createHref,
  title = "Add a notification channel",
  headingLevel: Heading = "h2",
}: {
  createHref: string;
  title?: string;
  headingLevel?: "h1" | "h2";
}) {
  return (
    <section className="flex flex-col gap-4" aria-labelledby="channel-type-picker">
      <div className="space-y-1">
        <Heading
          id="channel-type-picker"
          className={cn(
            "font-heading",
            Heading === "h1" ? "text-2xl" : "text-lg",
          )}
        >
          {title}
        </Heading>
        <p className="text-muted-foreground text-sm">
          Choose where alerts go when one of your monitors goes down.
        </p>
      </div>

      <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {channelTypeOptions.map((option) => (
          <li key={option.type}>
            <ChannelTypeCard option={option} createHref={createHref} />
          </li>
        ))}
      </ul>
    </section>
  );
}

function ChannelTypeCard({
  option,
  createHref,
}: {
  option: ChannelTypeOption;
  createHref: string;
}) {
  const label = channelTypeLabels[option.type];
  const available = option.createPath !== null;

  const content = (
    <>
      <div className="flex items-center gap-3">
        <span className="flex size-8 shrink-0 items-center justify-center rounded-lg border bg-background">
          <ChannelTypeIcon type={option.type} className="size-4" />
        </span>
        <span className="font-medium">{label}</span>
        {available ? null : (
          <Badge variant="outline" size="sm" className="ms-auto">
            Coming soon
          </Badge>
        )}
      </div>
      <p className="text-muted-foreground text-sm">{option.description}</p>
    </>
  );

  const className = "h-full gap-3 p-4";

  return available ? (
    <Card
      className={cn(
        className,
        "outline-none transition-colors hover:bg-accent/40 focus-visible:ring-2 focus-visible:ring-ring",
      )}
      render={<Link href={`${createHref}/${option.createPath}`} />}
    >
      {content}
    </Card>
  ) : (
    <Card
      aria-disabled
      className={cn(className, "cursor-not-allowed opacity-56 shadow-none")}
    >
      {content}
    </Card>
  );
}
