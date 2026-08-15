## Purpose

Lets a developer bring up the entire NutriCheck stack (database, API, web app) with a single command, so local development and manual verification don't require hand-wiring each service.

## ADDED Requirements

### Requirement: One-command local stack startup
Running `docker-compose up` from the `infra/` directory SHALL start three services — Postgres, the API, and the web app — as a single local stack, without requiring any manually-run setup steps beyond providing environment values.

#### Scenario: Developer starts the stack
- **WHEN** a developer with Docker installed runs `docker-compose up` in `infra/`
- **THEN** a Postgres container, the API container, and the web app container all start successfully

### Requirement: Configurable via environment template
The local stack SHALL read its configuration (database credentials, service ports, service URLs) from environment variables, with an `.env.template` at the repo root documenting every variable required to run it.

#### Scenario: Developer configures the stack from the template
- **WHEN** a developer copies `.env.template` to `.env` and fills in values
- **THEN** `docker-compose up` uses those values to configure Postgres, the API, and the web app without additional code changes

### Requirement: API reachable from the web app and host
Once the stack is up, the API SHALL be reachable both from the host machine (for manual testing) and from the web app container (for future server-side calls), on a documented port.

#### Scenario: Host machine calls the API
- **WHEN** the stack is running and a developer sends `GET /health` to the documented host port
- **THEN** the API responds with HTTP 200

### Requirement: Web app reachable from the host
Once the stack is up, the web app SHALL be reachable from the host machine's browser on a documented port.

#### Scenario: Developer opens the web app
- **WHEN** the stack is running and a developer navigates to the documented web app URL in a browser
- **THEN** the web app's home page loads successfully
