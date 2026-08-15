import { cookies } from "next/headers";
import { API_INTERNAL_URL, proxyApiRequest } from "./api";

export type Session = { authenticated: boolean; email: string | null };

export async function getSession(): Promise<Session> {
  const cookieHeader = (await cookies()).toString();

  const response = await fetch(`${API_INTERNAL_URL}/auth/me`, {
    headers: cookieHeader ? { Cookie: cookieHeader } : {},
    cache: "no-store",
  });

  if (!response.ok) {
    return { authenticated: false, email: null };
  }

  return (await response.json()) as Session;
}

export const proxyAuthRequest = proxyApiRequest;
