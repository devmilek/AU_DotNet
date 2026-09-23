import { Logo } from "@/components/logo";
import { GalleryVerticalEnd, HeartPulse } from "lucide-react";
import Image from "next/image";
import Link from "next/link";

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="grid min-h-svh lg:grid-cols-2">
      <div className="flex flex-col gap-4 p-6 md:p-10 relative">
        <div className="flex flex-1 items-center justify-center relative">
          <div className="w-full max-w-sm">{children}</div>
        </div>
      </div>
      <div className="bg-muted relative hidden lg:block">
        <Image
          src="/authentication-bg.png"
          alt="Image"
          width={1000}
          height={1000}
          className="object-fill h-full"
        />
        <Link
          href="/"
          className="flex items-center gap-2 font-medium absolute top-6 left-6"
        >
          <Logo className="size-8" />
          <span className="font-heading text-lg">Asterio Uptime</span>
        </Link>
        <div className="absolute bottom-0 px-8 pb-8">
          <p>
            “This library has saved me countless hours of work and helped me
            deliver stunning designs to my clients faster than ever before.” -
            Sofia Davis
          </p>
        </div>
      </div>
    </div>
  );
}
