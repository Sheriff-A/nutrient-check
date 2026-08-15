## MODIFIED Requirements

### Requirement: One-command local stack startup
Running `docker-compose up` from the `infra/` directory SHALL start three services — Postgres, the API, and the web app — as a single local stack, without requiring any manually-run setup steps beyond providing environment values. The API SHALL be configured with a Postgres connection string derived from the same credentials the `postgres` service uses (via the Docker network hostname, not `localhost`), so it can connect to the database without additional manual wiring.

#### Scenario: Developer starts the stack
- **WHEN** a developer with Docker installed runs `docker-compose up` in `infra/`
- **THEN** a Postgres container, the API container, and the web app container all start successfully

#### Scenario: API connects to Postgres using docker-compose-provided credentials
- **WHEN** the stack is running
- **THEN** the API can open a connection to Postgres using the connection string `docker-compose` provided, without requiring the developer to manually edit application configuration
