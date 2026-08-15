import { cookies } from "next/headers";
import { NextRequest, NextResponse } from "next/server";

const API_INTERNAL_URL = process.env.API_INTERNAL_URL ?? "http://api:8080";

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

export async function proxyAuthRequest(request: NextRequest, apiPath: string): Promise<NextResponse> {
  const cookieHeader = request.headers.get("cookie");
  const hasBody = request.method !== "GET" && request.method !== "HEAD";

  const apiResponse = await fetch(`${API_INTERNAL_URL}${apiPath}`, {
    method: request.method,
    headers: {
      ...(hasBody ? { "Content-Type": "application/json" } : {}),
      ...(cookieHeader ? { Cookie: cookieHeader } : {}),
    },
    body: hasBody ? await request.text() : undefined,
    cache: "no-store",
  });

  const responseBody = await apiResponse.text();
  const response = new NextResponse(responseBody, {
    status: apiResponse.status,
    headers: { "Content-Type": apiResponse.headers.get("Content-Type") ?? "application/json" },
  });

  for (const cookie of apiResponse.headers.getSetCookie()) {
    response.headers.append("Set-Cookie", cookie);
  }

  return response;
}
