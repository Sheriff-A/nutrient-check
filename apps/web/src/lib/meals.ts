import { cookies } from "next/headers";
import { API_INTERNAL_URL } from "./api";
import type { Meal } from "./meal-types";

export async function getMeals(): Promise<Meal[]> {
  const cookieHeader = (await cookies()).toString();

  const response = await fetch(`${API_INTERNAL_URL}/meals`, {
    headers: cookieHeader ? { Cookie: cookieHeader } : {},
    cache: "no-store",
  });

  if (!response.ok) {
    return [];
  }

  return (await response.json()) as Meal[];
}
