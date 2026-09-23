"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { api } from "@/lib/api/client";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { InfoIcon } from "lucide-react";

export function ResendForm({ defaultEmail = "" }: { defaultEmail?: string }) {
  const [email, setEmail] = useState(defaultEmail);
  const [state, setState] = useState<"idle" | "sending" | "sent" | "throttled">(
    "idle",
  );

  async function resend() {
    setState("sending");
    const { response } = await api.POST("/api/auth/resend-confirmation", {
      body: { email },
    });
    setState(response.status === 429 ? "throttled" : "sent");
  }

  if (state === "sent") {
    return (
      <Alert>
        <InfoIcon />
        <AlertTitle>Wiadomość wysłana</AlertTitle>
        <AlertDescription>
          Jeśli konto o tym adresie istnieje i nie jest jeszcze potwierdzone,
          wysłaliśmy nową wiadomość.
        </AlertDescription>
      </Alert>
    );
  }

  return (
    <div className="space-y-3">
      {!defaultEmail && (
        <Input
          type="email"
          autoComplete="email"
          placeholder="twoj@email.pl"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
      )}
      <Button
        className="w-full"
        variant="outline"
        disabled={state === "sending" || !email}
        onClick={resend}
      >
        {state === "sending" ? "Wysyłanie..." : "Wyślij ponownie"}
      </Button>
      {state === "throttled" && (
        <p className="text-sm text-destructive">
          Zbyt wiele prób. Odczekaj chwilę.
        </p>
      )}
    </div>
  );
}
