## 1. EF Core / Npgsql wiring

- [x] 1.1 Add `Npgsql.EntityFrameworkCore.PostgreSQL` and `Microsoft.EntityFrameworkCore.Design` package references to `NutriCheck.Infrastructure`.
- [x] 1.2 Add an empty `NutriCheckDbContext` (no `DbSet`s yet) to `NutriCheck.Infrastructure`.
- [x] 1.3 Register `NutriCheckDbContext` in `NutriCheck.Api`'s `Program.cs` via `AddDbContext`, reading the connection string from `ConnectionStrings:Default`.
- [x] 1.4 Add a `ConnectionStrings:Default` placeholder to `appsettings.json`/`appsettings.Development.json` (empty or local-dev default, not a real secret).
- [x] 1.5 Verify the API fails to start with a clear error when `ConnectionStrings:Default` is missing (per `specs/database-migrations/spec.md` - "API fails fast with no connection string configured").

## 2. Migrations tooling

- [x] 2.1 Add a `dotnet-ef` local tool manifest (`.config/dotnet-tools.json`) to `apps/api`.
- [x] 2.2 Generate an initial migration (`dotnet ef migrations add InitialCreate`) in `NutriCheck.Infrastructure/Migrations` — expected to contain no tables beyond EF Core's own migrations history table.
- [x] 2.3 Document the migration workflow (add/apply locally) in a README section under `apps/api`.

## 3. docker-compose wiring

- [x] 3.1 Update `infra/docker-compose.yml` so the `api` service's environment includes `ConnectionStrings__Default`, composed from the same `POSTGRES_DB`/`POSTGRES_USER`/`POSTGRES_PASSWORD` variables the `postgres` service uses, pointed at `Host=postgres`. (Already satisfied: `api`'s `env_file: ../.env` delivers `ConnectionStrings__Default`, pre-added to `.env.template` by `init-repo-scaffold` anticipating this change — verified, no edit needed.)
- [x] 3.2 Update `.env.template` if any new variables are introduced by the composition in 3.1. (No new variables needed — `ConnectionStrings__Default` already present.)

## 4. Verification

- [x] 4.1 Run `docker-compose up` in `infra/` and confirm all three containers start (per `specs/local-dev-stack/spec.md` - "Developer starts the stack"). (Verified: `docker compose up -d --build` started `infra-postgres-1`, `infra-api-1`, `infra-web-1`, API booted in `Production` mode using only the compose-provided connection string.)
- [x] 4.2 From the running stack, apply the initial migration (`dotnet ef database update` against the compose Postgres, or run it from inside the API container) and confirm it succeeds with no errors (per `specs/database-migrations/spec.md` - "Applying migrations to an empty database produces the current schema"). (Ran from inside the `api` container over the Docker network, since the host's port 5432 was contended by an unrelated native Postgres process on this machine — `docker compose exec api ... dotnet-ef database update` created `__EFMigrationsHistory` and applied `InitialCreate` successfully.)
- [x] 4.3 Re-run the same migration apply step and confirm it's a no-op (per `specs/database-migrations/spec.md` - "Applying migrations twice is a no-op"). (Verified: second run logged "No migrations were applied. The database is already up to date.")
- [x] 4.4 Confirm `GET /health` still returns HTTP 200 unauthenticated and unchanged (per `specs/database-migrations/spec.md`'s MODIFIED requirement - `/health` stays untouched). (Verified: `curl http://localhost:5080/health` → `200 OK`, `{"status":"healthy"}`.)
