import { NextRequest } from "next/server";
import { proxyApiRequest } from "@/lib/api";

export async function GET(request: NextRequest) {
  const { search } = new URL(request.url);
  return proxyApiRequest(request, `/meals${search}`);
}

export async function POST(request: NextRequest) {
  return proxyApiRequest(request, "/meals");
}
