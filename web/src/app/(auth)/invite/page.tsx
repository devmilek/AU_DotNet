import { InvitationView } from "@/modules/organizations/ui/invitation/invitation-view";

export default async function InvitePage({
  searchParams,
}: {
  searchParams: Promise<{ token?: string | string[] }>;
}) {
  const { token } = await searchParams;

  return <InvitationView token={(Array.isArray(token) ? token[0] : token) ?? ""} />;
}
