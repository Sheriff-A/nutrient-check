## Context

The API (`apps/api/src/NutriCheck.Api`) uses minimal APIs grouped with `MapGroup` (see `Auth/AuthEndpoints.cs`), with request/response DTOs as `record`s in a sibling `*Contracts.cs` file, and cookie authentication (`nutricheck.auth`) established by `user-auth`. No endpoint currently uses `[Authorize]`/`RequireAuthorization()` — `/auth/me` checks `http.User.Identity?.IsAuthenticated` manually because it must return a 200 either way. `NutriCheckDbContext` currently has a single `DbSet<User> Users`; migrations live in `NutriCheck.Infrastructure/Migrations` named `yyyyMMddHHmmss_PascalCaseDescription`. There is no test project yet for the API.

The web app (`apps/web/src/app`) is a Next.js App Router project with no UI/CSS framework beyond plain HTML + `globals.css`. `/profile` is an async Server Component that calls `getSession()` (`apps/web/src/lib/auth.ts`) and `redirect("/login")` when unauthenticated — this is the pattern to reuse for `/meals`. Auth API routes proxy to the backend via `proxyAuthRequest`, forwarding the session cookie in both directions. See proposal.md for motivation.

## Goals / Non-Goals

**Goals:**
- Persist meals with nutrient values, scoped to the owning user, via the existing EF Core + minimal API stack.
- Make `/meals` a functional, authenticated page consistent with how `/profile` already gates on session.
- Establish an ownership-check pattern (user can only touch their own meals) that later per-record capabilities can reuse.

**Non-Goals:**
- AI-powered nutrient analysis or estimation from photos/descriptions (called out as future work in `user-auth`'s purpose) — this change is manual entry only, with the caller supplying nutrient values directly.
- Aggregate/derived views (daily totals, charts, trends) — only CRUD on individual meal entries.
- A food/ingredient database or reusable food catalog — a meal is a single free-text entry with nutrient values, not a composition of catalog items.

## Decisions

**Minimal API + `RequireAuthorization()`, new pattern for this codebase.** Every meal endpoint requires an authenticated caller (unlike `/auth/me`, which must respond 200 when anonymous). Rather than repeating the manual `http.User.Identity?.IsAuthenticated` check in each handler, map the meals group with `.RequireAuthorization()` (cookie scheme already registered in `Program.cs`) so ASP.NET Core returns 401 before the handler runs. This is a small deviation from existing code (which has no prior case needing strict auth) but is the standard minimal-API mechanism and avoids duplicating the check five times.

**`Meal` as a flat entity, not a food-composition model.** Fields: `Id (Guid)`, `UserId (Guid, FK)`, `Name (string, required)`, `Description (string?, optional)`, `MealType (enum: Breakfast/Lunch/Dinner/Snack)`, `EatenAt (DateTimeOffset)`, `Calories/ProteinGrams/CarbsGrams/FatGrams (decimal, >= 0)`, `CreatedAt (DateTimeOffset)`. Alternative considered: modeling meals as a collection of food items each with their own nutrients, summed for totals. Rejected for this change as premature — no food catalog exists yet, and the proposal scopes this to direct manual entry; a food-composition model can be layered on later without breaking this shape (meal-level nutrient fields could become computed instead of stored).

**Ownership enforced in the application/query layer, not just at the DB.** Every read/update/delete filters by `UserId == currentUserId` (from the auth cookie's `NameIdentifier` claim, same claim `user-auth` already issues) before touching a record, and returns 404 (not 403) when a meal exists but belongs to someone else, so existence of other users' records is never revealed. Simple `WHERE` filtering rather than Postgres row-level security — the app already brokers all DB access, and RLS would be new infrastructure for a single-table concern.

**Listing uses simple `take`/`skip` query params, not cursor pagination.** Meals are listed ordered by `EatenAt desc`. `take` defaults to a fixed page size and is capped at a max; `skip` defaults to 0. Cursor-based pagination is unnecessary complexity at this stage (no data volume yet, no infinite-scroll UI planned in this change).

**Web: `/meals` becomes an async Server Component like `/profile`, with a client-side form island.** The page server-fetches the session and the meal list (redirecting to `/login` if unauthenticated, matching `/profile`), and renders a small `"use client"` form component for adding a meal that calls a new Next.js route handler and then refreshes the list (e.g. `router.refresh()`), avoiding a full page reload per the web-app-shell delta spec. New Next.js API routes `apps/web/src/app/api/meals/route.ts` (GET list, POST create) and `apps/web/src/app/api/meals/[id]/route.ts` (GET, PATCH, DELETE) proxy to the backend the same way `proxyAuthRequest` does today; that helper will be generalized (or a sibling helper added) so it isn't auth-specific.

## Risks / Trade-offs

- [No API test project exists yet, so meal endpoints would ship untested at the integration level] → Add a minimal `NutriCheck.Api.Tests` (or similar) project as part of this change rather than deferring test infrastructure to a later change, since ownership-enforcement bugs (leaking another user's meal) are exactly the kind of thing worth an automated check.
- [Storing nutrient values directly on the meal (not derived) means no validation that they're realistic] → Accept for this change; only non-negativity is enforced. Plausibility/estimation is explicitly out of scope (see Non-Goals).
- [`RequireAuthorization()` introduces a second auth-enforcement style alongside `/auth/me`'s manual check] → Acceptable: the two endpoints have genuinely different requirements (must-be-authenticated vs. may-be-anonymous), so one shared style would force a workaround either way.
