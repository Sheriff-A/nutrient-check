## Context

The stack runs as two separate containers with different host-visible origins (`web` on `:3000`, `api` on `:5080` per `infra/docker-compose.yml`), and locally everything is plain HTTP, not HTTPS. `NutriCheckDbContext` currently has no entities; `Program.cs` has no auth middleware. See [proposal.md](proposal.md) for motivation and [specs/user-auth/spec.md](specs/user-auth/spec.md) for behavior.

## Goals / Non-Goals

**Goals:**
- A session mechanism that works correctly across the web/API origin split, in both local Docker dev (HTTP) and a future deployed environment (HTTPS).
- Password storage that's safe by default with no custom crypto.
- Keep the surface area small: one user type, no roles, no password reset/email verification yet.

**Non-Goals:**
- Social/OAuth login, multi-factor auth, password reset flows, email verification — future changes.
- Role-based authorization — every authenticated user has the same access in this change.
- Profile editing — `/profile` only proves the session works; editable fields come later.

## Decisions

### Cookie carried browser→web only; web proxies to the API server-to-server
The browser never talks to the API's origin directly. The web app's Next.js route handlers (`/api/auth/*` under `apps/web`) accept the request, forward it server-side to the API (`http://api:8080/auth/*` inside the Docker network), and relay the `Set-Cookie` header back to the browser scoped to the web app's own origin.

**Why:** the API and web app are on different host ports/origins. A cookie issued directly by the API to the browser would need `SameSite=None; Secure` to survive a cross-site fetch, and `Secure` cookies are dropped by browsers over plain HTTP — which is exactly how local `docker-compose up` runs today. Proxying through the web app means the cookie is always first-party from the browser's perspective (`SameSite=Lax` is enough), so it works locally over HTTP today and continues working once a deployed environment adds HTTPS, with no cookie-flag branching by environment.

**Alternative considered:** issue the cookie directly from the API with `SameSite=None; Secure` and require HTTPS even in local dev (e.g. via a local reverse proxy or self-signed certs). Rejected — it adds TLS setup to local dev purely to satisfy a browser cookie rule, contradicting `local-dev-stack`'s one-command-startup goal.

### Server-side identity + opaque session cookie, not a client-readable JWT
The API issues an HTTP-only cookie containing an opaque session identifier (via ASP.NET Core's cookie authentication handler, backed by its default data-protection-encrypted ticket). The browser and the web app's client-side code never see a decodable token.

**Why:** the web app's own server-side code (route handlers, server components) is the only thing that needs to read auth state, and it does so by forwarding the browser's cookie to `GET /auth/me` rather than decoding a JWT locally. This avoids shipping a JWT signing key to two apps, avoids client-side token storage (a common XSS exposure point), and logout is a real server-side invalidation instead of "hope the client discards the token."

**Alternative considered:** JWT in an HTTP-only cookie, validated independently by both API and (if needed later) web app. Rejected as unnecessary complexity for a single-API-backend setup — JWKS/key distribution and expiry/refresh logic buys nothing here since there's only one verifier (the API itself).

### Password hashing via ASP.NET Core's built-in `PasswordHasher<T>`
Use the framework's `PasswordHasher<TUser>` (PBKDF2 with a per-password salt, versioned so the work factor can be upgraded later) rather than adopting full ASP.NET Core Identity or a third-party library.

**Why:** it gives safe defaults (salting, iteration count, algorithm agility) without pulling in ASP.NET Core Identity's much larger surface (role management, external login providers, email tokens) that this change doesn't need. `NutriCheckDbContext` stays a plain EF Core context with one new `Users` table rather than inheriting `IdentityDbContext`.

**Alternative considered:** full ASP.NET Core Identity. Rejected for now as heavier than the current requirements; nothing here blocks adopting it later if roles/external login become necessary — the `Users` table and hashing approach are compatible with that migration path.

## Risks / Trade-offs

- **[Risk]** Proxying auth through the web app adds a hop and means the web app's server must be trusted to relay cookies faithfully. → Mitigation: the proxy route handlers do no logic beyond forwarding request/response and the `Set-Cookie`/`Cookie` headers; no session data is parsed or stored in the web app.
- **[Risk]** Opaque server-side sessions mean the API must stay reachable for every auth check (no offline/stateless verification). → Mitigation: acceptable at current scale (single API instance, session check is a fast in-memory/cookie-ticket validation, not a DB round trip) — reconsider only if the API needs to scale to multiple stateless replicas without shared session storage.
- **[Risk]** `PasswordHasher<T>`'s default work factor may need tuning over time as hardware improves. → Mitigation: the hasher is versioned and supports rehashing on login when the configured iteration count changes; not needed at initial launch.

## Migration Plan

1. Add `User` entity + EF Core migration (additive; no existing tables touched).
2. Add auth endpoints and cookie authentication middleware to the API (additive; `/health` and existing behavior unaffected).
3. Add the web app's `/api/auth/*` proxy route handlers.
4. Replace the `/login` stub and add the `/profile` auth gate.
5. No data backfill needed — this is a net-new table with no prior users to migrate.
6. Rollback: revert the migration (drops the new table) and the endpoint/route changes; no other capability depends on this yet.
