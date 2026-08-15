## 1. Repo layout

- [x] 1.1 Create top-level directories: `apps/api`, `apps/web`, `docs/`, `infra/`, `.github/workflows/` (per PLANNING.md Section 7 / proposal.md)
- [x] 1.2 Add `.env.template` at the repo root documenting variables for Postgres, API, and web (connection string pieces, ports, `NEXT_PUBLIC_API_URL`)
- [x] 1.3 Add/update root `.gitignore` for .NET (`bin/`, `obj/`) and Node (`node_modules/`, `.next/`) build artifacts

## 2. ASP.NET Core solution (4-project layering)

- [x] 2.1 Create `apps/api/src/NutriCheck.Domain` (class library, no project references)
- [x] 2.2 Create `apps/api/src/NutriCheck.Application` (class library, references `NutriCheck.Domain`)
- [x] 2.3 Create `apps/api/src/NutriCheck.Infrastructure` (class library, references `NutriCheck.Application` and `NutriCheck.Domain`)
- [x] 2.4 Create `apps/api/src/NutriCheck.Api` (ASP.NET Core Web API, references `NutriCheck.Application` and `NutriCheck.Infrastructure`)
- [x] 2.5 Create `apps/api/NutriCheck.sln` (or equivalent) including all four projects
- [x] 2.6 Verify the solution builds cleanly (`dotnet build`) with no circular or reversed project references

## 3. Health check endpoint

- [x] 3.1 Wire `GET /health` in `NutriCheck.Api` (minimal API or controller) returning HTTP 200 with a JSON status body, per `specs/health-check/spec.md`
- [x] 3.2 Confirm the endpoint requires no authentication and has no dependency on database connectivity
- [x] 3.3 Manually verify: `dotnet run` then `curl http://localhost:<port>/health` returns 200

## 4. Next.js app shell

- [x] 4.1 Scaffold `apps/web` as a Next.js (App Router) + TypeScript app
- [x] 4.2 Add stub page `app/login/page.tsx` rendering placeholder content, no data fetching
- [x] 4.3 Add stub page `app/profile/page.tsx` rendering placeholder content, no data fetching
- [x] 4.4 Add stub page `app/meals/page.tsx` rendering placeholder content, no data fetching
- [x] 4.5 Verify each route (`/login`, `/profile`, `/meals`) resolves without a runtime error (`npm run dev`, visit each in browser), per `specs/web-app-shell/spec.md`

## 5. Local dev stack (docker-compose)

- [x] 5.1 Create `infra/docker-compose.yml` with three services: `postgres` (image `postgres:16`, named volume, port `5432`), `api`, `web`
- [x] 5.2 Add a `Dockerfile` for `apps/api` (dev-oriented, not production-hardened) used by the `api` service
- [x] 5.3 Add a `Dockerfile` for `apps/web` (dev-oriented) used by the `web` service
- [x] 5.4 Wire environment variables from `.env`/`.env.template` into all three services (DB connection string for `api` pointing at the `postgres` service by Docker network name; `NEXT_PUBLIC_API_URL=http://localhost:5080` for `web`)
- [x] 5.5 Expose `api` on host port `5080` and `web` on host port `3000`
- [x] 5.6 Verify: `docker-compose up` from `infra/` brings up all three containers; `curl http://localhost:5080/health` returns 200; `http://localhost:3000` loads in a browser, per `specs/local-dev-stack/spec.md`

## 6. CI pipeline

- [x] 6.1 Create `.github/workflows/ci.yml` triggered on pull requests targeting `main`
- [x] 6.2 Add an `api` job: `actions/setup-dotnet`, `dotnet restore`, `dotnet build`, and a lint/format check (e.g. `dotnet format --verify-no-changes`) for `apps/api`
- [x] 6.3 Add a `web` job: `actions/setup-node`, `npm ci`, `npm run build`, `npm run lint` for `apps/web`
- [x] 6.4 Confirm both jobs run in parallel (no unnecessary `needs:` dependency between them)
- [ ] 6.5 Open a throwaway PR (or push to a branch) to confirm both jobs run and report status correctly, per `specs/ci-build-lint/spec.md`; confirm a deliberately broken build/lint fails the corresponding job

## 7. Wrap-up verification

- [ ] 7.1 Confirm PLANNING.md Section 8 Phase 0 "Done when" condition: `docker-compose up` gives a working empty app end to end
- [ ] 7.2 Confirm CI is green on a real PR for this change
- [ ] 7.3 Run `openspec validate init-repo-scaffold --strict` and fix any reported issues
