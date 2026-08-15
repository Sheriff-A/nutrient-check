## Why

The repo scaffold (`init-repo-scaffold`) deliberately left Postgres as an empty, unused container — the API has no EF Core, no `DbContext`, and no connection string wiring. Every feature change that needs to persist data (`add-auth`, `add-meal-logging`, ...) needs a working migrations pipeline to build on, so that work should land once, here, rather than being reinvented (or diverging) in each feature change.

## What Changes

- Add EF Core + Npgsql to `NutriCheck.Infrastructure`, with a `NutriCheckDbContext` (no entities yet — this change ships the plumbing, not the domain model).
- Wire the API's Postgres connection string through configuration (`appsettings.json` placeholder + `ConnectionStrings__Default` from environment), sourced from the same Postgres credentials `docker-compose` already provisions.
- Add EF Core migrations tooling (`dotnet-ef`) and an initial migration that applies cleanly against the `docker-compose` Postgres instance, proving the pipeline end-to-end even with zero domain tables.
- Document the migration workflow (how to add/apply migrations locally and what runs in CI/deploy) in `apps/api`.

Explicitly out of scope: any domain entities (`User`, `Meal`, `MealItem`, nutrient data, etc.) — those are modeled by the feature changes that need them (`add-auth`, `add-meal-logging`, ...), each adding its own migration on top of this foundation. Also out of scope: changing `/health` — it stays process-liveness-only per its existing spec (`specs/health-check/spec.md`); DB connectivity is proven by the migration pipeline itself, not baked into that endpoint.

## Capabilities

### New Capabilities
- `database-migrations`: the API is backed by an EF Core `DbContext` connected to Postgres via configuration, with a migrations pipeline (create, apply, verify) that works locally and is ready for CI/deploy use.

### Modified Capabilities
- `local-dev-stack`: the `api` service in `docker-compose` is configured with a Postgres connection string (via the Docker network hostname) so the API can actually reach the database, not just start alongside it.

## Impact

- **New code:** `NutriCheck.Infrastructure` (DbContext, EF Core/Npgsql packages, initial migration), `NutriCheck.Api` (connection string configuration).
- **Modified code:** `apps/api/src/NutriCheck.Api/appsettings*.json`, `infra/docker-compose.yml` (connection string env var for `api`), `.env.template` (new Postgres connection variables if needed).
- **Dependencies introduced:** `Microsoft.EntityFrameworkCore.Design`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `dotnet-ef` CLI tool.
- **No impact** on the web app, auth, or any business/domain logic — those remain separate, later changes.
