import { SignUpForm } from "@/modules/auth/ui/components/sign-up-form";
import { SocialsSign } from "@/modules/auth/ui/components/socials-sign";
import Link from "next/link";

const SignUpPage = async ({
  searchParams,
}: {
  searchParams: Promise<{ email?: string | string[] }>;
}) => {
  const { email } = await searchParams;

  // const { user } = await getCurrentSession();
  // if (user) {
  //   redirect("/");
  // }

  return (
    <div className="space-y-6">
      <div className="flex flex-col items-center gap-1 text-center">
        <h1 className="text-2xl font-heading">Create an account</h1>
        <p className="text-muted-foreground text-sm text-balance">
          Enter your details below to create your account
        </p>
      </div>
      <SignUpForm defaultEmail={Array.isArray(email) ? email[0] : email} />
      <SocialsSign />
      <p className="text-sm text-center text-muted-foreground">
        Already have an account?{" "}
        <Link href="/sign-in" className="text-primary font-medium">
          Sign in
        </Link>
      </p>
    </div>
  );
};

export default SignUpPage;
