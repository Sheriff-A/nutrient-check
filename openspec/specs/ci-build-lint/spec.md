# ci-build-lint Specification

## Purpose

Gives every pull request an automated, required pass/fail signal that both apps still build and pass lint, catching breakage before merge without a human running local checks.

## Requirements

### Requirement: CI runs on every pull request
A GitHub Actions workflow SHALL run automatically on every pull request targeting the main branch, building and linting both `apps/api` and `apps/web`.

#### Scenario: PR opened against main
- **WHEN** a pull request is opened or updated against the main branch
- **THEN** the GitHub Actions workflow triggers and runs both the API and web jobs

### Requirement: API build and lint step
The workflow SHALL restore and build the `apps/api` ASP.NET Core solution, failing the job if the build or lint step fails.

#### Scenario: API fails to build
- **WHEN** a pull request introduces a compile error in `apps/api`
- **THEN** the API job in the workflow fails and is reported as a failing check on the pull request

### Requirement: Web build and lint step
The workflow SHALL install dependencies, build, and lint the `apps/web` Next.js app, failing the job if the build or lint step fails.

#### Scenario: Web app fails lint
- **WHEN** a pull request introduces a lint violation in `apps/web`
- **THEN** the web job in the workflow fails and is reported as a failing check on the pull request

#### Scenario: Both apps pass
- **WHEN** a pull request's changes build and lint cleanly for both `apps/api` and `apps/web`
- **THEN** both jobs report success and the workflow as a whole passes
