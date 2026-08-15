## Why

NutriCheck currently has no repository structure to build against — no solution files, no frontend scaffold, no local dev stack, no CI. Phase 0 of PLANNING.md (Section 8) calls for a working, empty, end-to-end skeleton (`docker-compose up` runs the full stack, CI is green) before any real feature work (auth, meal logging, AI review) starts. This change delivers that skeleton so subsequent changes (`init-db-schema`, `add-auth`, ...) have a consistent foundation to build on.

## What Changes

- Establish the monorepo layout from PLANNING.md Section 7: `apps/api`, `apps/web`, `docs/`, `infra/`, `.github/workflows/`.
- Add an ASP.NET Core 8 solution under `apps/api` with the 4-project Clean Architecture split: `NutriCheck.Domain`, `NutriCheck.Application`, `NutriCheck.Infrastructure`, `NutriCheck.Api` (empty layers, project references wired per the dependency direction, no business logic yet).
- Add a basic health check endpoint (`GET /health`) on the API project.
- Add a Next.js (App Router, TypeScript) scaffold under `apps/web` with routing stubbed for `/login`, `/profile`, and meal logging (`/meals`) — placeholder pages only, no implementation.
- Add `infra/docker-compose.yml` running Postgres, the API, and the web app locally.
- Add a GitHub Actions workflow (`.github/workflows/`) that builds and lints both `apps/api` and `apps/web` on pull request.
- Add `.env.template` at the repo root for local configuration.

Explicitly out of scope (deferred to later changes per PLANNING.md Section 12): database schema/migrations (`init-db-schema`), authentication, and any business logic.

## Capabilities

### New Capabilities
- `health-check`: the API exposes an unauthenticated health check endpoint reporting service status, used by docker-compose/CI/ops to verify the API is up.
- `local-dev-stack`: `docker-compose up` in `infra/` brings up Postgres, the API, and the web app together for local development, with the API and web reachable on documented ports.
- `ci-build-lint`: a GitHub Actions workflow runs build and lint for both `apps/api` and `apps/web` on every pull request, giving a pass/fail signal before merge.
- `web-app-shell`: the Next.js app resolves stubbed routes for login, profile, and meal logging, each rendering placeholder content without a runtime error.

### Modified Capabilities
None — this is the first change in the repo; no existing specs to modify.

## Impact

- **New code:** `apps/api/**` (solution + 4 projects), `apps/web/**` (Next.js app), `infra/docker-compose.yml`, `.github/workflows/ci.yml`, `.env.template`.
- **New docs:** none required by this change beyond what's scaffolded (architecture/ADR docs are separate, per PLANNING.md Section 10).
- **Dependencies introduced:** ASP.NET Core 8 SDK, Next.js + TypeScript toolchain, Docker/docker-compose, Postgres image.
- **No impact** on database schema, auth, or business logic — those are separate, later changes.
