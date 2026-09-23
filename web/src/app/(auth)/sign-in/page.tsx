import { SignInForm } from "@/modules/auth/ui/components/sign-in-form";
import { SocialsSign } from "@/modules/auth/ui/components/socials-sign";
import Link from "next/link";

const SignInPage = async () => {
  // const { user } = await getCurrentSession();
  // if (user) {
  //   redirect("/");
  // }

  return (
    <div className="space-y-6">
      <div className="flex flex-col items-center gap-1 text-center">
        <h1 className="text-2xl font-heading">Login to your account</h1>
        <p className="text-muted-foreground text-sm text-balance">
          Enter your email below to login to your account
        </p>
      </div>
      <SignInForm />
      <SocialsSign />
      <p className="text-sm text-center text-muted-foreground">
        Don&apos;t have an account?{" "}
        <Link href="/sign-up" className="text-primary font-medium">
          Sign up
        </Link>
      </p>
    </div>
  );
};

export default SignInPage;
