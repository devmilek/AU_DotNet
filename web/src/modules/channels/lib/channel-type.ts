import type { components } from "@/lib/api/schema";

type ChannelType = components["schemas"]["ChannelType"];
type ChannelConfig = components["schemas"]["ChannelConfigResponse"];

export const channelTypeLabels: Record<ChannelType, string> = {
  Email: "Email",
  Discord: "Discord",
  Slack: "Slack",
  Webhook: "Webhook",
};

/** Krótki opis celu kanału, np. "ops@acme.com +2". */
export function describeChannelTarget(config: ChannelConfig): string {
  const to = config.email?.to ?? [];
  if (to.length === 0) return "—";
  return to.length === 1 ? to[0] : `${to[0]} +${to.length - 1}`;
}

export type ChannelTypeOption = {
  type: ChannelType;
  description: string;
  createPath: string | null;
};

export const channelTypeOptions: ChannelTypeOption[] = [
  {
    type: "Email",
    description: "Send alerts to one or more email addresses.",
    createPath: "email",
  },
  {
    type: "Webhook",
    description: "POST incident events to your own endpoint.",
    createPath: null,
  },
  {
    type: "Slack",
    description: "Post alerts to a Slack channel.",
    createPath: null,
  },
  {
    type: "Discord",
    description: "Post alerts to a Discord channel.",
    createPath: null,
  },
];
