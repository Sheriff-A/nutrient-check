import { NextRequest } from "next/server";
import { proxyAuthRequest } from "@/lib/auth";

export async function GET(request: NextRequest) {
  return proxyAuthRequest(request, "/auth/me");
}
