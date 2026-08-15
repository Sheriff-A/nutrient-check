## 1. Domain & Persistence

- [x] 1.1 Add `MealType` enum (Breakfast, Lunch, Dinner, Snack) and `Meal` entity (`Id`, `UserId`, `Name`, `Description?`, `MealType`, `EatenAt`, `Calories`, `ProteinGrams`, `CarbsGrams`, `FatGrams`, `CreatedAt`) to `NutriCheck.Domain`.
- [x] 1.2 Add `DbSet<Meal> Meals` to `NutriCheckDbContext` and configure the entity in `OnModelCreating` (required fields, FK to `User`, index on `(UserId, EatenAt)`).
- [x] 1.3 Generate and review the EF Core migration adding the `Meals` table.
- [x] 1.4 Apply the migration locally against the dev Postgres (via `infra/docker-compose.yml`) and confirm the schema matches the model.

## 2. API: Meal Endpoints

- [x] 2.1 Add `MealContracts.cs` with request/response records: `CreateMealRequest`, `UpdateMealRequest`, `MealResponse`, `ErrorResponse` reuse from Auth if suitable.
- [x] 2.2 Add `MealEndpoints.cs` with a `MapMealEndpoints` extension mapping a `/meals` group with `.RequireAuthorization()`, and register it in `Program.cs`.
- [x] 2.3 Implement `POST /meals`: validate required fields and non-negative nutrient values, create the meal for the current user (from the `NameIdentifier` claim), return 201 with the created meal.
- [x] 2.4 Implement `GET /meals`: return the current user's meals ordered by `EatenAt desc`, honoring `take`/`skip` query params with a default and max page size.
- [x] 2.5 Implement `GET /meals/{id}`: return the meal if owned by the current user, else 404.
- [x] 2.6 Implement `PATCH /meals/{id}`: apply the same validation as create, update only if owned by the current user, else 404.
- [x] 2.7 Implement `DELETE /meals/{id}`: delete only if owned by the current user, else 404.

## 3. API Tests

- [x] 3.1 Create a `NutriCheck.Api.Tests` project (test framework matching the team's default, e.g. xUnit) wired into `NutriCheck.sln`.
- [x] 3.2 Add tests covering: create success, create validation failures (missing field, negative nutrient), list returns only own meals, get/update/delete on an owned meal succeeds, get/update/delete on another user's meal returns 404, all endpoints reject unauthenticated requests.

## 4. Web: API Proxy Routes

- [x] 4.1 Generalize `proxyAuthRequest` (or add a sibling helper) in `apps/web/src/lib` so it can proxy arbitrary authenticated JSON requests, not just `/auth/*`.
- [x] 4.2 Add `apps/web/src/app/api/meals/route.ts` proxying `GET`/`POST` to the backend `/meals`.
- [x] 4.3 Add `apps/web/src/app/api/meals/[id]/route.ts` proxying `GET`/`PATCH`/`DELETE` to the backend `/meals/{id}`.

## 5. Web: Meal Log Page

- [x] 5.1 Convert `apps/web/src/app/meals/page.tsx` into an async Server Component that calls `getSession()`, redirects to `/login` if unauthenticated, and server-fetches the user's meal list.
- [x] 5.2 Add a `"use client"` add-meal form component (name, description, meal type, eaten-at, calories, protein, carbs, fat) that posts to `/api/meals` and refreshes the list on success, showing validation errors returned by the API.
- [x] 5.3 Render the fetched meals as a list (name, meal type, eaten-at, nutrient values) with a delete action per meal calling `/api/meals/[id]`.

## 6. Verification

- [x] 6.1 Run the API test suite.
- [x] 6.2 Manually verify in the browser: logging in, adding a meal, seeing it listed, deleting it, and confirming `/meals` redirects to `/login` when signed out.
- [x] 6.3 Run `openspec validate meal-logging --strict` and fix any reported issues.
