export type MealType = 0 | 1 | 2 | 3;

// Ordinal values must match NutriCheck.Domain.MealType (Breakfast=0, Lunch=1, Dinner=2, Snack=3).
export const MEAL_TYPE_LABELS: Record<MealType, string> = {
  0: "Breakfast",
  1: "Lunch",
  2: "Dinner",
  3: "Snack",
};

export type Meal = {
  id: string;
  name: string;
  description: string | null;
  mealType: MealType;
  eatenAt: string;
  calories: number;
  proteinGrams: number;
  carbsGrams: number;
  fatGrams: number;
  createdAt: string;
};
