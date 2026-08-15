"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { MEAL_TYPE_LABELS, type MealType } from "@/lib/meal-types";

const MEAL_TYPE_OPTIONS = (Object.entries(MEAL_TYPE_LABELS) as [string, string][]).map(([value, label]) => ({
  value: Number(value) as MealType,
  label,
}));

function toLocalDateTimeInputValue(date: Date): string {
  const offsetMs = date.getTimezoneOffset() * 60 * 1000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}

export default function AddMealForm() {
  const router = useRouter();
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [mealType, setMealType] = useState<MealType>(0);
  const [eatenAt, setEatenAt] = useState(() => toLocalDateTimeInputValue(new Date()));
  const [calories, setCalories] = useState("0");
  const [protein, setProtein] = useState("0");
  const [carbs, setCarbs] = useState("0");
  const [fat, setFat] = useState("0");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      const response = await fetch("/api/meals", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          name,
          description: description || null,
          mealType,
          eatenAt: new Date(eatenAt).toISOString(),
          calories: Number(calories),
          proteinGrams: Number(protein),
          carbsGrams: Number(carbs),
          fatGrams: Number(fat),
        }),
      });

      if (!response.ok) {
        const data = await response.json().catch(() => null);
        setError(data?.error ?? "Something went wrong. Please try again.");
        return;
      }

      setName("");
      setDescription("");
      setCalories("0");
      setProtein("0");
      setCarbs("0");
      setFat("0");
      router.refresh();
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label htmlFor="name">Name</label>
        <input id="name" required value={name} onChange={(event) => setName(event.target.value)} />
      </div>

      <div>
        <label htmlFor="description">Description</label>
        <input id="description" value={description} onChange={(event) => setDescription(event.target.value)} />
      </div>

      <div>
        <label htmlFor="mealType">Meal type</label>
        <select
          id="mealType"
          value={mealType}
          onChange={(event) => setMealType(Number(event.target.value) as MealType)}
        >
          {MEAL_TYPE_OPTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      <div>
        <label htmlFor="eatenAt">Eaten at</label>
        <input
          id="eatenAt"
          type="datetime-local"
          required
          value={eatenAt}
          onChange={(event) => setEatenAt(event.target.value)}
        />
      </div>

      <div>
        <label htmlFor="calories">Calories</label>
        <input
          id="calories"
          type="number"
          min="0"
          step="any"
          required
          value={calories}
          onChange={(event) => setCalories(event.target.value)}
        />
      </div>

      <div>
        <label htmlFor="protein">Protein (g)</label>
        <input
          id="protein"
          type="number"
          min="0"
          step="any"
          required
          value={protein}
          onChange={(event) => setProtein(event.target.value)}
        />
      </div>

      <div>
        <label htmlFor="carbs">Carbs (g)</label>
        <input
          id="carbs"
          type="number"
          min="0"
          step="any"
          required
          value={carbs}
          onChange={(event) => setCarbs(event.target.value)}
        />
      </div>

      <div>
        <label htmlFor="fat">Fat (g)</label>
        <input
          id="fat"
          type="number"
          min="0"
          step="any"
          required
          value={fat}
          onChange={(event) => setFat(event.target.value)}
        />
      </div>

      {error && <p role="alert">{error}</p>}

      <button type="submit" disabled={submitting}>
        Add meal
      </button>
    </form>
  );
}
