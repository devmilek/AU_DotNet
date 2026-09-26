import {
  Select,
  SelectItem,
  SelectPopup,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { assignableRoles, type OrganizationRole } from "../../lib/roles";

export function RoleSelect({
  actorRole,
  value,
  onValueChange,
  disabled,
  className,
  "aria-label": ariaLabel,
}: {
  actorRole: OrganizationRole;
  value: OrganizationRole;
  onValueChange: (role: OrganizationRole) => void;
  disabled?: boolean;
  className?: string;
  "aria-label"?: string;
}) {
  const options = assignableRoles(actorRole);
  const items = options.map((option) => ({
    value: option.value,
    label: option.label,
  }));

  return (
    <Select
      items={items}
      value={value}
      disabled={disabled}
      onValueChange={(next) => {
        if (typeof next === "number") onValueChange(next);
      }}
    >
      <SelectTrigger className={className} aria-label={ariaLabel}>
        <SelectValue />
      </SelectTrigger>
      <SelectPopup>
        {options.map((option) => (
          <SelectItem key={option.value} value={option.value}>
            <span className="flex flex-col">
              <span>{option.label}</span>
              <span className="text-muted-foreground text-xs">
                {option.description}
              </span>
            </span>
          </SelectItem>
        ))}
      </SelectPopup>
    </Select>
  );
}
