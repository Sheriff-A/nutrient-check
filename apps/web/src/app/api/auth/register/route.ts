import { NextRequest } from "next/server";
import { proxyAuthRequest } from "@/lib/auth";

export async function POST(request: NextRequest) {
  return proxyAuthRequest(request, "/auth/register");
}
