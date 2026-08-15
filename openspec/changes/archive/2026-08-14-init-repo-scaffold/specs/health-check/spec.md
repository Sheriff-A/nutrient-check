## Purpose

Gives operators, orchestrators (docker-compose, CI, future Cloud Run), and developers a single, unauthenticated way to confirm the API process is up and accepting requests.

## ADDED Requirements

### Requirement: API health check endpoint
The API SHALL expose an unauthenticated `GET /health` endpoint that returns HTTP 200 with a JSON body indicating the service is healthy when the process is running and able to handle requests.

#### Scenario: API is running
- **WHEN** a client sends `GET /health` to a running API instance
- **THEN** the API responds with HTTP 200 and a JSON body containing a status field indicating "healthy"

#### Scenario: Health check requires no authentication
- **WHEN** a client sends `GET /health` without any credentials or auth token
- **THEN** the API responds with HTTP 200, not HTTP 401 or 403

#### Scenario: Health check does not depend on the database
- **WHEN** a client sends `GET /health` before any database connection has been established
- **THEN** the API still responds with HTTP 200, since this endpoint only reports process liveness, not downstream dependency health
