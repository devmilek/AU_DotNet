import { redirect } from "next/navigation";

export default async function OrganizationHomePage({
  params,
}: {
  params: Promise<{ organizationSlug: string }>;
}) {
  const { organizationSlug } = await params;
  redirect(`/${organizationSlug}/monitors`);
}
