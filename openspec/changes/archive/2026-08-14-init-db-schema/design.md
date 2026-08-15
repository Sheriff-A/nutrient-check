## Context

See `proposal.md` - Why/What Changes. The repo scaffold (`init-repo-scaffold`) left `NutriCheck.Infrastructure` empty and Postgres running unused in `docker-compose`. This change adds the EF Core/Npgsql plumbing and migrations pipeline; it does not model any domain entity. `specs/database-migrations/spec.md` and the `local-dev-stack` delta in `specs/local-dev-stack/spec.md` define the resulting behavior.

## Goals / Non-Goals

**Goals:**
- Give every subsequent feature change (`add-auth`, `add-meal-logging`, ...) a working `DbContext` + migrations pipeline to add entities and migrations to, rather than each one wiring EF Core independently.
- Prove the pipeline end-to-end against the real `docker-compose` Postgres instance (an initial migration applies cleanly), not just that the code compiles.
- Keep `/health` unchanged (process-liveness-only, per its existing spec) — this change deliberately does not couple the health endpoint to DB/migration state.

**Non-Goals:**
- No domain entities (`User`, `Meal`, `MealItem`, nutrient data). Those belong to the feature changes that need them.
- No production deployment/migration-runner wiring (e.g. how migrations apply in a deployed environment) — that's Phase 4 per `PLANNING.md`; this change only needs the pipeline to work locally and be invokable in CI later.
- No connection resiliency policy (retry-on-failure, pooling tuning) beyond EF Core/Npgsql defaults.

## Decisions

### DbContext lives in `NutriCheck.Infrastructure`, registered from `NutriCheck.Api`
`NutriCheckDbContext` is added to `NutriCheck.Infrastructure` (it already depends on `Application`/`Domain` per the existing layering) and registered via `AddDbContext` in `NutriCheck.Api`'s composition root (`Program.cs`), reading the connection string from configuration. This follows the same dependency direction already established by the scaffold — `Api` composes `Infrastructure`, `Infrastructure` doesn't know about `Api`.

Alternative considered: put the `DbContext` in `Api` directly, since there are no entities yet to justify a separate layer. Rejected — entities and their configuration will land in `Infrastructure` as soon as `add-auth`/`add-meal-logging` need them, and moving the `DbContext` later would just be churn; placing it correctly now costs nothing.

### Connection string sourced from `ConnectionStrings:Default`, built by docker-compose from the existing Postgres env vars
`docker-compose.yml` already defines `POSTGRES_DB`/`POSTGRES_USER`/`POSTGRES_PASSWORD` for the `postgres` service. Rather than introduce a second, separately-configured connection string that could drift from those values, the `api` service's environment gets `ConnectionStrings__Default` composed from the same `POSTGRES_*` variables, pointed at the `postgres` service by Docker network hostname (`Host=postgres`), consistent with the "reach Postgres by service name, not `localhost`" note already in `docker-compose.yml`. Local (non-Docker) runs of the API — e.g. `dotnet run` against a developer's own Postgres — set `ConnectionStrings__Default` via `appsettings.Development.json` or user secrets, documented alongside the migration workflow.

Alternative considered: a fully independent `DATABASE_URL`-style variable. Rejected — it would duplicate credentials that already exist as `POSTGRES_*`, risking the two falling out of sync; composing the connection string from the existing variables keeps a single source of truth.

### Migrations authored and applied via `dotnet-ef`, checked into `NutriCheck.Infrastructure/Migrations`
Standard EF Core workflow: `dotnet ef migrations add <Name>` generates migration files checked into source control; `dotnet ef database update` (or `context.Database.Migrate()`) applies them. This change adds the `dotnet-ef` local tool manifest and one migration (empty schema, i.e. just the EF Core migrations history table) to prove the round trip. How migrations get applied in a deployed environment (startup `Migrate()` call vs. a separate migration-runner step in CI/CD) is left as an open question below — not needed to prove the pipeline locally.

Alternative considered: a raw-SQL migration tool (e.g. Flyway, dbmate) instead of EF Core migrations. Rejected — the API is EF Core's natural consumer, code-first migrations keep schema and C# entity changes in the same PR/commit, and introducing a second schema-authoring language (SQL) alongside EF Core's C#-first model adds tooling surface without a concrete need driving it.

### `/health` stays untouched
Per `specs/database-migrations/spec.md`'s MODIFIED requirement (restating the existing `health-check` scenario), `/health` continues to report only process liveness. DB connectivity is instead proven by the migration pipeline itself (`dotnet ef database update` succeeding is the observable signal), keeping this change from having to touch the `health-check` capability at all.

## Risks / Trade-offs

- **[Risk]** Composing the API's connection string from the same `POSTGRES_*` variables the `postgres` service uses means a typo in `docker-compose.yml`'s composition logic silently produces a connection string that doesn't match. → **Mitigation**: the "API connects to Postgres using docker-compose-provided credentials" scenario in `specs/local-dev-stack/spec.md` is directly verifiable by starting the stack and running a migration/query; this is the manual verification step for this change.
- **[Risk]** Shipping a migrations pipeline with zero domain tables means the first real migration (added by `add-auth` or `add-meal-logging`) is the first one to exercise "add a table," not just "create the history table" — slightly reduces how much this change actually proves. → **Mitigation**: acceptable; the goal is the pipeline (tooling, connection, apply mechanism), not any specific schema, and an empty migration still exercises the full apply path against real Postgres.
- **[Risk]** No decision yet on how migrations apply in deployed environments risks a later change having to retrofit this. → **Mitigation**: tracked as an open question below; local dev and CI don't need it resolved yet, and the decision is additive (a startup hook or CI step), not a rework of what this change builds.

## Migration Plan

Additive only — no existing schema or data to migrate (Postgres is currently empty and unused). Rollback, if ever needed, is `git revert` of this change's commit(s) plus `dotnet ef database update <PreviousMigration>` (or dropping the local dev database) to undo the one migration this change adds.

## Open Questions

- How migrations apply in a deployed environment (API runs `Database.Migrate()` on startup vs. a dedicated CI/CD migration step) — deferred to the change that first ships a deploy pipeline (Phase 4 per `PLANNING.md`); doesn't affect this change's specs, approach, or tasks.
