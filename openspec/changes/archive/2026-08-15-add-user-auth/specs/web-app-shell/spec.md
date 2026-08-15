## MODIFIED Requirements

### Requirement: Login route exists
The web app SHALL resolve a `/login` route that renders a functional login and registration form, submitting credentials to the API and redirecting to `/profile` on success.

#### Scenario: Visiting the login route
- **WHEN** a user navigates to `/login`
- **THEN** the app renders a login/registration form, not a 404 or error page

#### Scenario: Logging in successfully
- **WHEN** a user submits valid credentials on `/login`
- **THEN** the app establishes an authenticated session and redirects the user to `/profile`

#### Scenario: Logging in with invalid credentials
- **WHEN** a user submits incorrect credentials on `/login`
- **THEN** the app remains on `/login` and displays an error, without establishing a session

### Requirement: Profile route exists
The web app SHALL resolve a `/profile` route that requires an authenticated session: authenticated users see a placeholder profile page identifying their account, and unauthenticated visitors are redirected to `/login`.

#### Scenario: Visiting the profile route while authenticated
- **WHEN** an authenticated user navigates to `/profile`
- **THEN** the app renders a placeholder profile page showing the authenticated user's account, not a 404 or error page

#### Scenario: Visiting the profile route while unauthenticated
- **WHEN** an unauthenticated visitor navigates to `/profile`
- **THEN** the app redirects the visitor to `/login` instead of rendering the profile page

### Requirement: No functional behavior implied by stub routes
Routes that remain stubs SHALL contain only placeholder content (e.g. a heading naming the page) and SHALL NOT implement forms, data fetching, or auth logic — that behavior belongs to later, separate changes. `/login` and `/profile` are no longer stub routes as of the `user-auth` capability; this requirement applies to `/meals` and any future stub routes only.

#### Scenario: Stub page has no data dependency
- **WHEN** `/meals` is rendered with no backend API running
- **THEN** the page still renders successfully, since it makes no network calls
