import { NextRequest } from "next/server";
import { proxyApiRequest } from "@/lib/api";

type RouteParams = { params: Promise<{ id: string }> };

export async function GET(request: NextRequest, { params }: RouteParams) {
  const { id } = await params;
  return proxyApiRequest(request, `/meals/${id}`);
}

export async function PATCH(request: NextRequest, { params }: RouteParams) {
  const { id } = await params;
  return proxyApiRequest(request, `/meals/${id}`);
}

export async function DELETE(request: NextRequest, { params }: RouteParams) {
  const { id } = await params;
  return proxyApiRequest(request, `/meals/${id}`);
}
