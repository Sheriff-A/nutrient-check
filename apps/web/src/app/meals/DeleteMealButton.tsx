"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";

export default function DeleteMealButton({ mealId }: { mealId: string }) {
  const router = useRouter();
  const [deleting, setDeleting] = useState(false);

  async function handleDelete() {
    setDeleting(true);
    try {
      await fetch(`/api/meals/${mealId}`, { method: "DELETE" });
      router.refresh();
    } finally {
      setDeleting(false);
    }
  }

  return (
    <button type="button" onClick={handleDelete} disabled={deleting}>
      Delete
    </button>
  );
}
