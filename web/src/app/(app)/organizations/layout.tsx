import { Logo } from "@/components/logo";
import Link from "next/link";

export default function OrganizationsLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-svh flex-col">
      <header className="flex items-center gap-2 px-6 py-4">
        <Link href="/" className="flex items-center gap-2 font-medium">
          <Logo className="size-8" />
          <span className="font-heading text-lg">Asterio Uptime</span>
        </Link>
      </header>
      <main className="flex flex-1 items-center justify-center px-6 pb-16">
        <div className="w-full max-w-md">{children}</div>
      </main>
    </div>
  );
}
