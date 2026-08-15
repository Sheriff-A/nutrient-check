# NutriCheck.Api

ASP.NET Core 8 API, 4-project Clean Architecture split (`NutriCheck.Domain`, `NutriCheck.Application`, `NutriCheck.Infrastructure`, `NutriCheck.Api`).

## Database migrations

The API is backed by Postgres via EF Core (`NutriCheckDbContext` in `NutriCheck.Infrastructure`). Migrations are the only supported way to change the schema — never modify a database directly.

Migration commands run from `apps/api` using the `dotnet-ef` local tool (installed via `dotnet tool restore`, manifest at `apps/api/.config/dotnet-tools.json`).

### Adding a migration

After changing `NutriCheckDbContext` or its entity configuration:

```bash
cd apps/api
dotnet tool restore
dotnet tool run dotnet-ef migrations add <MigrationName> \
  --project src/NutriCheck.Infrastructure \
  --startup-project src/NutriCheck.Api \
  --output-dir Migrations
```

This generates files under `src/NutriCheck.Infrastructure/Migrations/` — check them in.

### Applying migrations locally

Requires a reachable Postgres and a valid `ConnectionStrings:Default` for the startup project (see `.env.template` at the repo root for the value used by `docker-compose`; `appsettings.Development.json` has the equivalent for running the API directly on the host against `localhost`):

```bash
cd apps/api
dotnet tool run dotnet-ef database update \
  --project src/NutriCheck.Infrastructure \
  --startup-project src/NutriCheck.Api
```

Running it again after all migrations are already applied is a no-op.

### Removing an unapplied migration

```bash
dotnet tool run dotnet-ef migrations remove \
  --project src/NutriCheck.Infrastructure \
  --startup-project src/NutriCheck.Api
```

### CI / deploy

How migrations get applied outside local dev (startup hook vs. a dedicated pipeline step) isn't decided yet — see `openspec/changes/init-db-schema/design.md` (Open Questions).
