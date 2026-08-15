import { NextRequest, NextResponse } from "next/server";

export const API_INTERNAL_URL = process.env.API_INTERNAL_URL ?? "http://api:8080";

export async function proxyApiRequest(request: NextRequest, apiPath: string): Promise<NextResponse> {
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
  const isNoBodyStatus = apiResponse.status === 204 || apiResponse.status === 205;
  const response = new NextResponse(isNoBodyStatus ? null : responseBody, {
    status: apiResponse.status,
    headers: { "Content-Type": apiResponse.headers.get("Content-Type") ?? "application/json" },
  });

  for (const cookie of apiResponse.headers.getSetCookie()) {
    response.headers.append("Set-Cookie", cookie);
  }

  return response;
}
