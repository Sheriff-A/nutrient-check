## Purpose

Gives the API a schema evolution pipeline — a database context connected to Postgres and a migrations mechanism — that every future feature needing persistence builds on, instead of each one wiring EF Core from scratch.

## ADDED Requirements

### Requirement: API connects to Postgres via configuration
The API SHALL obtain its Postgres connection string from configuration (environment variable or configuration provider), never hard-coded, so the same build can point at different databases (local, CI, deployed) without a code change.

#### Scenario: API starts with a configured connection string
- **WHEN** the API process starts with a valid Postgres connection string present in its configuration
- **THEN** the API successfully establishes a connection to that Postgres database

#### Scenario: API fails fast with no connection string configured
- **WHEN** the API process starts with no Postgres connection string present in its configuration
- **THEN** the API fails to start with an error identifying the missing configuration, rather than starting and failing later on first database access

### Requirement: Schema changes are expressed as migrations
All Postgres schema changes SHALL be expressed as ordered, version-controlled migrations, applied to reach the current schema — no schema change SHALL be made directly against a database outside the migration mechanism.

#### Scenario: Applying migrations to an empty database produces the current schema
- **WHEN** all migrations are applied, in order, to a freshly created empty Postgres database
- **THEN** the resulting schema matches the schema produced by the current codebase, with no manual intervention

#### Scenario: Applying migrations twice is a no-op
- **WHEN** migrations are applied to a database that already has all of them applied
- **THEN** the operation completes successfully and makes no further schema changes
