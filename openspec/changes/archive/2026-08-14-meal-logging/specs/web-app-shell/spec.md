## MODIFIED Requirements

### Requirement: Meal logging route exists
The web app SHALL resolve a `/meals` route that requires an authenticated session: authenticated users see a functional meal log — a form to add a meal and a list of their previously logged meals — and unauthenticated visitors are redirected to `/login`.

#### Scenario: Visiting the meal logging route while authenticated
- **WHEN** an authenticated user navigates to `/meals`
- **THEN** the app renders the meal log, including a form to add a meal and a list of that user's logged meals

#### Scenario: Visiting the meal logging route while unauthenticated
- **WHEN** an unauthenticated visitor navigates to `/meals`
- **THEN** the app redirects the visitor to `/login` instead of rendering the meal log

#### Scenario: Logging a meal from the meal log page
- **WHEN** an authenticated user submits the add-meal form with valid meal details
- **THEN** the app creates the meal and displays it in the user's meal list without a full page reload being required to see it

### Requirement: No functional behavior implied by stub routes
Routes that remain stubs SHALL contain only placeholder content (e.g. a heading naming the page) and SHALL NOT implement forms, data fetching, or auth logic — that behavior belongs to later, separate changes. `/login`, `/profile`, and `/meals` are no longer stub routes; this requirement applies only to any future stub routes introduced by later changes.

#### Scenario: A future stub page has no data dependency
- **WHEN** a newly introduced stub route is rendered with no backend API running
- **THEN** the page still renders successfully, since it makes no network calls
