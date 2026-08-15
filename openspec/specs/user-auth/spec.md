# user-auth Specification

## Purpose

Gives the platform a single account and session mechanism so every other capability (profile, meal logging, AI analysis history) can identify which user a record belongs to.

## Requirements

### Requirement: User can register an account
The system SHALL allow a visitor to create an account with an email address and password, rejecting registration if the email is already registered or the password does not meet a minimum length.

#### Scenario: Successful registration
- **WHEN** a visitor submits registration with an email not already in use and a password meeting the minimum length
- **THEN** the system creates the account, hashes and stores the password (never storing it in plain text or a reversible form), and the visitor is registered

#### Scenario: Duplicate email rejected
- **WHEN** a visitor submits registration with an email address that already has an account
- **THEN** the system rejects the registration with an error identifying the email is already in use, and no duplicate account is created

#### Scenario: Weak password rejected
- **WHEN** a visitor submits registration with a password shorter than the minimum length
- **THEN** the system rejects the registration with an error identifying the password requirement, and no account is created

### Requirement: User can log in with a session established
The system SHALL allow a registered user to authenticate with their email and password and, on success, establish a session so subsequent requests are recognized as that user.

#### Scenario: Successful login
- **WHEN** a registered user submits their correct email and password
- **THEN** the system establishes a session for that user and future requests carrying that session are treated as authenticated

#### Scenario: Incorrect credentials rejected
- **WHEN** a user submits an email and password that do not match a registered account
- **THEN** the system rejects the login with a generic authentication error that does not reveal whether the email exists

### Requirement: User can log out, ending the session
The system SHALL allow an authenticated user to end their session, after which requests are no longer recognized as that user.

#### Scenario: Successful logout
- **WHEN** an authenticated user requests logout
- **THEN** the system ends the session, and a subsequent request carrying the former session is treated as unauthenticated

### Requirement: Session can be verified
The system SHALL expose a way for a client to check whether the current request is authenticated and, if so, identify the authenticated user.

#### Scenario: Checking an authenticated session
- **WHEN** a client makes a session-check request carrying a valid, active session
- **THEN** the system responds identifying the authenticated user

#### Scenario: Checking an unauthenticated session
- **WHEN** a client makes a session-check request with no session or an expired/invalid session
- **THEN** the system responds indicating the request is not authenticated, without error

### Requirement: Sessions do not leak across users
The system SHALL ensure a session is tied to exactly one user account and cannot be used, forged, or extended to act as a different user.

#### Scenario: Tampered session rejected
- **WHEN** a request carries a session token that has been altered or is not one the system issued
- **THEN** the system treats the request as unauthenticated rather than attributing it to any user
