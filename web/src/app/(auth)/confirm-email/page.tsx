import { ConfirmToken } from "@/modules/auth/ui/components/confirm-token";
import { ResendForm } from "@/modules/auth/ui/components/resend-form";

export default async function ConfirmEmailPage({
  searchParams,
}: {
  searchParams: Promise<{ token?: string; userId?: string; email?: string }>;
}) {
  const { token, userId, email } = await searchParams;

  if (token && userId) {
    return <ConfirmToken token={token} userId={userId} />;
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col items-center gap-1 text-center">
        <h1 className="font-heading text-2xl">Potwierdź adres email</h1>
        <p className="text-muted-foreground text-sm text-balance">
          Wysłaliśmy wiadomość z linkiem potwierdzającym. Sprawdź skrzynkę,
          również folder ze spamem.
        </p>
      </div>

      <ResendForm defaultEmail={email} />
    </div>
  );
}
