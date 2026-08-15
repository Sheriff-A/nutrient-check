## Context

See `proposal.md` - Why/What Changes for motivation and scope. This design covers the repo-wide scaffolding described in PLANNING.md Sections 7 and 8: monorepo layout, the 4-project ASP.NET Core Clean Architecture split, the Next.js route shell, the local docker-compose stack, and the CI workflow. Nothing here touches the database schema (`init-db-schema`, a separate change), auth, or business logic.

## Goals / Non-Goals

**Goals:**
- Produce a repo layout and toolchain skeleton that later changes (`init-db-schema`, `add-auth`, `add-meal-logging`, ...) can build directly into without restructuring.
- Make the 4-project layering's dependency direction explicit and enforced by project references from day one, so it's mechanically hard to violate later even though the layers are currently empty.
- Keep docker-compose and CI minimal but real (no placeholder "echo build succeeded" steps) so `docker-compose up` and CI both exercise the actual toolchains.

**Non-Goals:**
- No production Dockerfiles or GCP wiring (Phase 4 in PLANNING.md).
- No EF Core / migrations / Postgres schema (separate `init-db-schema` change) — Postgres runs in docker-compose only as an empty database instance.
- No design decisions about auth, meal domain, or AI pipeline shape.
- No test projects beyond what's needed to prove the solution builds (xUnit/Vitest test scaffolding is introduced with the features they test, per PLANNING.md Section 9).

## Decisions

### Monorepo layout follows PLANNING.md Section 7 verbatim
`apps/api`, `apps/web`, `docs/`, `infra/`, `openspec/` (already present), `.github/workflows/`, `.env.template` at root. No deviation — this was already decided in PLANNING.md; restating it here would just duplicate it. The only addition: `docs/decisions/` is left for the first ADR-writing change, not created empty by this change, since an empty directory has no value and git doesn't track it anyway.

### 4-project layering with enforced reference direction
`NutriCheck.Domain` has no project references (POCOs, interfaces only). `NutriCheck.Application` references `Domain` only. `NutriCheck.Infrastructure` references `Application` and `Domain`. `NutriCheck.Api` references `Application` and `Infrastructure` (composition root — registers DI). This mirrors standard Clean Architecture dependency inversion and matches PLANNING.md Section 7's stated intent ("light Clean Architecture... enough to show layering, not so much ceremony a 4-entity app feels absurd").

Alternative considered: single `NutriCheck.Api` project with folders instead of projects. Rejected because PLANNING.md explicitly calls for the 4-project split as a portfolio signal (Section 7), and project references are what make the boundary enforced rather than a naming convention.

### Health check lives in `NutriCheck.Api`, not a separate layer
`GET /health` is wired directly in `Program.cs` (minimal API) rather than routed through Application/Infrastructure, since it currently reports only process liveness (see `specs/health-check/spec.md` — explicitly does not depend on the database). This keeps it trivial now; if it later needs to report DB connectivity (e.g. `AddHealthChecks().AddNpgSql(...)`), that's a small, additive change localized to `Api`.

### docker-compose service topology and ports
Three services: `postgres`, `api`, `web`. Postgres uses the standard `postgres:16` image with a named volume for data persistence across restarts, exposed on the conventional `5432` (documented, overridable via `.env`). API exposed on `5080` (host) → container's Kestrel port; web on `3000`. The API container's `ConnectionStrings__Default` (or equivalent) points at the `postgres` service by Docker network name, not `localhost` — this is the one non-obvious gotcha worth documenting inline in `docker-compose.yml` since it trips up people used to running Postgres on the host. The web container is not given a hardcoded API URL at build time; it reads `NEXT_PUBLIC_API_URL` from environment so the same image works against docker-compose or a future deployed API.

Alternative considered: web app calling the API via the browser using the API's host-exposed port (`localhost:5080`) rather than needing a separate in-network URL. This is simpler for a browser-only frontend (no server-side API calls yet), so we use `localhost:5080` for `NEXT_PUBLIC_API_URL` in the compose file rather than the Docker-network hostname, since `NEXT_PUBLIC_*` vars are inlined into browser-bound JS and the browser runs outside the Docker network.

### CI workflow: two independent jobs, not one
`.github/workflows/ci.yml` defines separate `api` and `web` jobs (parallel, not sequential) so a failure in one is clearly attributable and both run concurrently rather than doubling PR feedback latency. Each job does setup → restore/install → build → lint, using the respective ecosystem's standard actions (`actions/setup-dotnet`, `actions/setup-node`). No deploy step — that's Phase 4.

### Next.js route stubs are literal empty-shell pages
Each of `/login`, `/profile`, `/meals` is a Next.js App Router page (`app/login/page.tsx`, etc.) rendering a heading and one sentence of placeholder text, no client-side state, no fetch calls, no shared layout logic beyond the default root layout. This satisfies `specs/web-app-shell/spec.md` (routes resolve, no runtime error, no data dependency) while leaving 100% of the actual UI work to the changes that own each domain (`add-auth`, meal logging, etc.).

## Risks / Trade-offs

- **[Risk]** Deciding project reference direction now, before any real logic exists, risks getting it wrong and having to unwind references later. → **Mitigation**: the direction (Domain ← Application ← Infrastructure ← Api) is the standard Clean Architecture arrangement and matches what PLANNING.md already commits to; low risk of needing to reverse it.
- **[Risk]** `NEXT_PUBLIC_API_URL=localhost:5080` baked into the web container only works when the browser and Docker host are the same machine (true for local dev, not for any future deployed setup). → **Mitigation**: this is local-dev-only per this change's scope (`local-dev-stack` capability); production wiring is explicitly Phase 4 and will use a different value, not a code change.
- **[Risk]** Two parallel CI jobs each pay independent toolchain setup/cache-cold cost, slower in aggregate compute than one combined job. → **Mitigation**: acceptable at this repo's size; caching (`actions/setup-dotnet`'s and `actions/setup-node`'s built-in cache options) mitigates most of it, and the clearer pass/fail attribution is worth the trade at this scale.

## Migration Plan

This is a greenfield scaffold (first change in the repo) — no existing state to migrate. Rollback, if ever needed, is `git revert` of the change's commit(s); nothing here is deployed or holds data yet (the Postgres volume is local-only and disposable).
