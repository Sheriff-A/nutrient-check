## 1. Domain and persistence

- [x] 1.1 Add `User` entity to `NutriCheck.Domain` (id, email, password hash, created timestamp)
- [x] 1.2 Add `Users` `DbSet<User>` to `NutriCheckDbContext`, with a unique index on email
- [x] 1.3 Add and apply an EF Core migration for the new `Users` table

## 2. API authentication plumbing

- [x] 2.1 Add ASP.NET Core cookie authentication to `Program.cs` (HTTP-only, `SameSite=Lax` session cookie)
- [x] 2.2 Add CORS configuration allowing the web app's origin, if any direct browser→API calls remain (otherwise confirm none are needed once the proxy is in place) — confirmed none needed: per design.md, the browser only ever calls the web app's own origin, which proxies to the API server-to-server; no CORS middleware added.
- [x] 2.3 Add `PasswordHasher<User>` service for hashing and verifying passwords

## 3. Auth endpoints

- [x] 3.1 `POST /auth/register` — validate email uniqueness and password length, hash password, create user
- [x] 3.2 `POST /auth/login` — verify credentials, sign in via cookie authentication, return generic error on failure
- [x] 3.3 `POST /auth/logout` — sign out, clearing the session cookie
- [x] 3.4 `GET /auth/me` — return the authenticated user's identity, or an unauthenticated response with no error

## 4. Web app proxy and pages

- [x] 4.1 Add Next.js route handlers under `apps/web/src/app/api/auth/*` that forward to the API's `/auth/*` endpoints and relay `Set-Cookie`/`Cookie` headers
- [x] 4.2 Replace the `/login` stub with a login/registration form that posts to the proxy routes and redirects to `/profile` on success, showing an error on failure
- [x] 4.3 Add a server-side auth check to `/profile` (calling the internal `/auth/me` endpoint via `getSession()` in `src/lib/auth.ts`, sharing logic with the `/api/auth/me` proxy route) that redirects unauthenticated visitors to `/login`
- [x] 4.4 Update the `/profile` placeholder to display the authenticated user's email

## 5. Verification

- [x] 5.1 Manually verify register → login → `/profile` → logout flow via `docker-compose up` (verified via the browser against the running stack: register → auto-switch to login → login redirects to `/profile` showing the account email → survives a full page reload → logout → `/profile` redirects back to `/login`)
- [x] 5.2 Verify an unauthenticated visit to `/profile` redirects to `/login` (verified both pre-login and post-logout)
- [x] 5.3 Verify duplicate registration, weak password, and bad login credentials all return the errors specified in `specs/user-auth/spec.md` (verified directly against the API: 409 for duplicate email, 400 for a too-short password, 401 with a generic message for bad credentials)
- [x] 5.4 Confirm `apps/api` and `apps/web` still build and lint cleanly (existing CI checks) (`dotnet build NutriCheck.sln` — 0 warnings/errors; `npm run lint` and `npm run build` — clean)
