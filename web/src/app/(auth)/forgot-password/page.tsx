import { ForgotPasswordForm } from "@/modules/auth/ui/components/forgot-password-form";
import Link from "next/link";

const ForgotPasswordPage = async () => {
  return (
    <div className="space-y-6">
      <div className="flex flex-col items-center gap-1 text-center">
        <h1 className="text-2xl font-heading">Forgot your password?</h1>
        <p className="text-muted-foreground text-sm text-balance">
          Enter your email and we&apos;ll send you a reset link
        </p>
      </div>
      <ForgotPasswordForm />
      <p className="text-sm text-center text-muted-foreground">
        Remembered your password?{" "}
        <Link href="/sign-in" className="text-primary font-medium">
          Sign in
        </Link>
      </p>
    </div>
  );
};

export default ForgotPasswordPage;
