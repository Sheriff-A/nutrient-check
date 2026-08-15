## Why

The web app has stub `/login` and `/profile` routes and the API has no notion of a user, so there is no way to identify who is logging a meal or viewing a profile. Every feature after this one (profile data, meal logging, the AI analysis history) needs a real account to attach records to. Authentication is the smallest slice that unblocks all of them, and the foundation (Postgres + EF Core migrations, docker-compose stack, CI) is already in place to build it on.

## What Changes

- Add a `Users` entity and EF Core migration to `NutriCheck.Domain` / `NutriCheck.Infrastructure` (email, hashed password, created timestamp).
- Add API endpoints on `NutriCheck.Api`: `POST /auth/register`, `POST /auth/login`, `POST /auth/logout`, `GET /auth/me`.
- Use ASP.NET Core cookie authentication: the API issues an HTTP-only, `Secure`, `SameSite=None` session cookie on login, backed by server-side identity (no client-readable JWT). CORS is configured to allow credentialed requests from the web app's origin.
- Hash passwords with ASP.NET Core's `PasswordHasher<T>` (PBKDF2) — no plaintext or reversible storage.
- Replace the `/login` placeholder page with a real login/register form that calls the API and redirects on success.
- Gate `/profile` behind session state: unauthenticated visitors are redirected to `/login`; authenticated visitors see their email (still a placeholder beyond that — no profile editing in this change).
- Add minimal session-check plumbing on the web app (a server-side fetch to `GET /auth/me` using the forwarded cookie) so pages can tell if a request is authenticated.

## Capabilities

### New Capabilities
- `user-auth`: registration, login, logout, and session verification for a single user account type (no roles/permissions in this change).

### Modified Capabilities
- `web-app-shell`: the `/login` route requirement changes from "renders a placeholder" to "renders a functional login/register form"; the `/profile` route requirement changes from "renders a placeholder" to "requires an authenticated session, redirecting unauthenticated visitors to `/login`".

## Impact

- **`apps/api/src/NutriCheck.Domain`**: new `User` entity.
- **`apps/api/src/NutriCheck.Infrastructure`**: `NutriCheckDbContext` gains a `Users` `DbSet`; new EF Core migration.
- **`apps/api/src/NutriCheck.Api`**: `Program.cs` adds cookie authentication, CORS-with-credentials, and the `/auth/*` endpoints.
- **`apps/web/src/app/login`**: stub replaced with a real form; new client-side fetch calls to the API.
- **`apps/web/src/app/profile`**: gains a server-side auth check and redirect.
- **`infra/docker-compose.yml` / `.env.template`**: may need a new API-side secret (cookie signing key) and confirmation that web→API requests carry cookies correctly across the two containers' origins.
- No breaking changes to existing endpoints (`/health` is untouched).
