import { redirect } from "next/navigation";
import { getSession } from "@/lib/auth";

export default async function ProfilePage() {
  const session = await getSession();

  if (!session.authenticated) {
    redirect("/login");
  }

  return (
    <main>
      <h1>Profile</h1>
      <p>Logged in as {session.email}.</p>
    </main>
  );
}
