import {
  NumberField,
  NumberFieldDecrement,
  NumberFieldGroup,
  NumberFieldIncrement,
  NumberFieldInput,
} from "@/components/ui/number-field";

export function NumberInput({
  value,
  onValueChange,
  onBlur,
  min,
  max,
  step = 1,
  invalid,
  disabled,
  className,
  "aria-label": ariaLabel,
}: {
  value: number;
  onValueChange: (value: number) => void;
  onBlur?: () => void;
  min?: number;
  max?: number;
  step?: number;
  invalid?: boolean;
  disabled?: boolean;
  className?: string;
  "aria-label"?: string;
}) {
  return (
    <NumberField
      className={className}
      value={Number.isNaN(value) ? null : value}
      onValueChange={(next) => onValueChange(next ?? Number.NaN)}
      min={min}
      max={max}
      step={step}
      disabled={disabled}
    >
      <NumberFieldGroup>
        <NumberFieldDecrement />
        <NumberFieldInput
          onBlur={onBlur}
          aria-label={ariaLabel}
          aria-invalid={invalid || undefined}
        />
        <NumberFieldIncrement />
      </NumberFieldGroup>
    </NumberField>
  );
}
