"use client";

import { Building2Icon, ImagePlusIcon } from "lucide-react";
import { useEffect, useId, useRef, useState } from "react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import {
  LOGO_ACCEPT,
  LOGO_MAX_BYTES,
  logoInitials,
  validateLogoFile,
} from "../lib/logo";

export type LogoSelection = { file: File; previewUrl: string };

type OrganizationLogoPickerProps = {
  name: string;
  value: LogoSelection | null;
  onChange: (value: LogoSelection | null) => void;
  disabled?: boolean;
};

export function OrganizationLogoPicker({
  name,
  value,
  onChange,
  disabled,
}: OrganizationLogoPickerProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [error, setError] = useState<string | null>(null);
  const hintId = useId();
  const initials = logoInitials(name);

  useEffect(() => {
    if (!value) return;
    return () => URL.revokeObjectURL(value.previewUrl);
  }, [value]);

  function openFilePicker() {
    inputRef.current?.click();
  }

  function handleFileChange(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) return;

    const validationError = validateLogoFile(file);
    if (validationError) {
      setError(validationError);
      return;
    }

    setError(null);
    onChange({ file, previewUrl: URL.createObjectURL(file) });
  }

  function handleRemove() {
    setError(null);
    onChange(null);
  }

  return (
    <div className="flex flex-col items-center gap-3">
      <button
        type="button"
        onClick={openFilePicker}
        disabled={disabled}
        aria-label={value ? "Change organization logo" : "Upload organization logo"}
        aria-describedby={hintId}
        className="group relative rounded-xl outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background disabled:opacity-64"
      >
        <Avatar className="size-20 rounded-xl border bg-muted">
          {value ? <AvatarImage src={value.previewUrl} alt="" /> : null}
          <AvatarFallback className="rounded-xl bg-muted font-heading text-2xl text-muted-foreground">
            {initials ?? <Building2Icon className="size-8 opacity-64" />}
          </AvatarFallback>
        </Avatar>
        <span className="absolute inset-0 flex items-center justify-center rounded-xl bg-black/48 text-white opacity-0 transition-opacity group-hover:opacity-100 group-focus-visible:opacity-100">
          <ImagePlusIcon className="size-6" />
        </span>
      </button>

      <input
        ref={inputRef}
        type="file"
        accept={LOGO_ACCEPT}
        className="sr-only"
        tabIndex={-1}
        aria-hidden
        onChange={handleFileChange}
      />

      <div className="flex items-center gap-2">
        <Button
          type="button"
          size="xs"
          variant="outline"
          onClick={openFilePicker}
          disabled={disabled}
        >
          {value ? "Change logo" : "Upload logo"}
        </Button>
        {value ? (
          <Button
            type="button"
            size="xs"
            variant="ghost"
            onClick={handleRemove}
            disabled={disabled}
          >
            Remove
          </Button>
        ) : null}
      </div>

      <p
        id={hintId}
        className={cn(
          "text-xs",
          error ? "text-destructive-foreground" : "text-muted-foreground",
        )}
      >
        {error ?? `PNG, JPG or WebP, up to ${LOGO_MAX_BYTES / 1024 / 1024} MB.`}
      </p>
    </div>
  );
}
