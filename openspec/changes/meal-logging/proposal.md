## Why

The platform can authenticate users and has a `/meals` stub route, but there is no way for a user to actually record what they eat. Meal logging is the core data-entry capability the rest of the product (nutrient totals, history, future AI analysis) depends on, so it needs to exist before any of those can be built.

## What Changes

- Add a `Meal` domain entity and EF Core migration: each meal belongs to exactly one user and records a name, optional description, meal type (breakfast/lunch/dinner/snack), eaten-at timestamp, and nutrient values (calories, protein, carbs, fat).
- Add authenticated API endpoints to create, list (with a bounded/paginated result), view, update, and delete a user's own meals.
- Replace the `/meals` placeholder page with a functional meal log: a form to add a meal and a list of the authenticated user's logged meals, redirecting unauthenticated visitors to `/login` (same pattern as `/profile`).
- Enforce per-user ownership: a user can only read, update, or delete their own meals.

## Capabilities

### New Capabilities
- `meal-logging`: create, list, view, update, and delete meal entries (with nutrient data) scoped to the authenticated owner.

### Modified Capabilities
- `web-app-shell`: the `/meals` route is no longer a stub — it becomes an authenticated page requiring a session, following the same gating pattern established for `/profile`.

## Impact

- **Domain/Infrastructure**: new `Meal` entity, `DbSet<Meal>`, and migration in `NutriCheck.Infrastructure`.
- **API**: new meal endpoints in `NutriCheck.Api`, gated by the existing session/auth mechanism; new application-layer logic in `NutriCheck.Application`.
- **Web**: `/meals` route in `apps/web/src/app` becomes functional; new API client calls alongside the existing auth calls in `apps/web/src/lib`.
- **Specs**: adds `meal-logging` spec; updates the `web-app-shell` spec's stub-route requirement for `/meals`.
