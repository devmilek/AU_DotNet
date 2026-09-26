"use client";

import { useEffect, useState } from "react";

/** Aktualny czas odświeżany co `intervalMs` — do etykiet typu "12 seconds ago". */
export function useNow(intervalMs = 1000): Date {
  const [now, setNow] = useState(() => new Date());

  useEffect(() => {
    const id = window.setInterval(() => setNow(new Date()), intervalMs);
    return () => window.clearInterval(id);
  }, [intervalMs]);

  return now;
}
