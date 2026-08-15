import { redirect } from "next/navigation";
import { getSession } from "@/lib/auth";
import { getMeals } from "@/lib/meals";
import { MEAL_TYPE_LABELS } from "@/lib/meal-types";
import AddMealForm from "./AddMealForm";
import DeleteMealButton from "./DeleteMealButton";

export default async function MealsPage() {
  const session = await getSession();

  if (!session.authenticated) {
    redirect("/login");
  }

  const meals = await getMeals();

  return (
    <main>
      <h1>Meal Log</h1>

      <AddMealForm />

      <h2>Logged Meals</h2>
      {meals.length === 0 ? (
        <p>No meals logged yet.</p>
      ) : (
        <ul>
          {meals.map((meal) => (
            <li key={meal.id}>
              <strong>{meal.name}</strong> — {MEAL_TYPE_LABELS[meal.mealType]} —{" "}
              {new Date(meal.eatenAt).toLocaleString()}
              <br />
              {meal.calories} kcal · {meal.proteinGrams}g protein · {meal.carbsGrams}g carbs · {meal.fatGrams}g fat
              <br />
              <DeleteMealButton mealId={meal.id} />
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
