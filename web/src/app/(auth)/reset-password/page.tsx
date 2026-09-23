import { CircleAlertIcon } from "lucide-react";
import Link from "next/link";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { ResetPasswordForm } from "@/modules/auth/ui/components/reset-password-form";

type ResetPasswordPageProps = {
  searchParams: Promise<{ token?: string | string[] }>;
};

const ResetPasswordPage = async ({ searchParams }: ResetPasswordPageProps) => {
  const params = await searchParams;
  const tokenValue = params.token;
  const token = Array.isArray(tokenValue) ? tokenValue[0] : tokenValue;

  return (
    <div className="space-y-6">
      <div className="flex flex-col items-center gap-1 text-center">
        <h1 className="text-2xl font-heading">Reset your password</h1>
        <p className="text-muted-foreground text-sm text-balance">
          Choose a new password for your account
        </p>
      </div>

      {token ? (
        <ResetPasswordForm token={token} />
      ) : (
        <div className="flex flex-col gap-4">
          <Alert variant="error">
            <CircleAlertIcon />
            <AlertDescription>
              This reset link is invalid or has expired. Request a new one to
              continue.
            </AlertDescription>
          </Alert>
          <Button className="w-full" render={<Link href="/forgot-password" />}>
            Request new link
          </Button>
        </div>
      )}

      <p className="text-sm text-center text-muted-foreground">
        Remembered your password?{" "}
        <Link href="/sign-in" className="text-primary font-medium">
          Sign in
        </Link>
      </p>
    </div>
  );
};

export default ResetPasswordPage;
