import {
  HashIcon,
  type LucideProps,
  MailIcon,
  MessageSquareIcon,
  WebhookIcon,
} from "lucide-react";
import type React from "react";
import type { components } from "@/lib/api/schema";

type ChannelType = components["schemas"]["ChannelType"];

const icons: Record<ChannelType, React.ComponentType<LucideProps>> = {
  Email: MailIcon,
  Webhook: WebhookIcon,
  Slack: HashIcon,
  Discord: MessageSquareIcon,
};

export function ChannelTypeIcon({
  type,
  ...props
}: { type: ChannelType } & LucideProps) {
  const Icon = icons[type];
  return <Icon {...props} />;
}
