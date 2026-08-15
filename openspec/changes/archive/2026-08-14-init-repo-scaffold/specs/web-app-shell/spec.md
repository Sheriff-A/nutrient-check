## Purpose

Establishes the Next.js app's route structure up front so later changes (auth, profile, meal logging) implement behavior inside existing pages instead of also inventing navigation and routing at the same time.

## ADDED Requirements

### Requirement: Login route exists
The web app SHALL resolve a `/login` route that renders a placeholder page without throwing a runtime error.

#### Scenario: Visiting the login route
- **WHEN** a user navigates to `/login`
- **THEN** the app renders a placeholder login page, not a 404 or error page

### Requirement: Profile route exists
The web app SHALL resolve a `/profile` route that renders a placeholder page without throwing a runtime error.

#### Scenario: Visiting the profile route
- **WHEN** a user navigates to `/profile`
- **THEN** the app renders a placeholder profile page, not a 404 or error page

### Requirement: Meal logging route exists
The web app SHALL resolve a `/meals` route that renders a placeholder page without throwing a runtime error.

#### Scenario: Visiting the meal logging route
- **WHEN** a user navigates to `/meals`
- **THEN** the app renders a placeholder meal logging page, not a 404 or error page

### Requirement: No functional behavior implied by stub routes
Stub routes SHALL contain only placeholder content (e.g. a heading naming the page) and SHALL NOT implement forms, data fetching, or auth logic — that behavior belongs to later, separate changes.

#### Scenario: Stub page has no data dependency
- **WHEN** any of `/login`, `/profile`, or `/meals` is rendered with no backend API running
- **THEN** the page still renders successfully, since it makes no network calls
