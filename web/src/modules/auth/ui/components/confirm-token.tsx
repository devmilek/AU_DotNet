"use client";

import { useEffect, useRef, useState } from "react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { api } from "@/lib/api/client";
import { ResendForm } from "./resend-form";

type Status = "pending" | "success" | "error";

export function ConfirmToken({
  token,
  userId,
}: {
  token: string;
  userId: string;
}) {
  const [status, setStatus] = useState<Status>("pending");
  const started = useRef(false);

  useEffect(() => {
    if (started.current) return;
    started.current = true;

    api
      .POST("/api/auth/confirm-email", { body: { token, userId } })
      .then(({ response }) => setStatus(response.ok ? "success" : "error"))
      .catch(() => setStatus("error"));
  }, [token, userId]);

  if (status === "pending") {
    return (
      <p className="text-muted-foreground text-center text-sm">
        Potwierdzamy adres email...
      </p>
    );
  }

  if (status === "success") {
    return (
      <div className="space-y-6 text-center">
        <h1 className="font-heading text-2xl">Adres potwierdzony</h1>
        <p className="text-muted-foreground text-sm">
          Możesz się teraz zalogować.
        </p>
        <Button render={<Link href="/sign-in" />} className="w-full">
          Przejdź do logowania
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6 text-center">
      <h1 className="font-heading text-2xl">Link jest nieprawidłowy</h1>
      <p className="text-muted-foreground text-sm">
        Link wygasł albo został już użyty. Wyślij nowy poniżej.
      </p>
      <ResendForm />
    </div>
  );
}
