# meal-logging Specification

## Purpose

Lets an authenticated user record what they ate — with nutrient values — and review, correct, or remove those records later, forming the factual history that nutrient totals and future analysis build on.

## Requirements

### Requirement: User can log a meal
The system SHALL allow an authenticated user to create a meal entry for themselves consisting of a name, an optional description, a meal type (breakfast, lunch, dinner, or snack), the date/time it was eaten, and nutrient values (calories, protein, carbohydrates, fat), rejecting the request if required fields are missing or nutrient values are negative.

#### Scenario: Successful meal creation
- **WHEN** an authenticated user submits a meal with a name, meal type, eaten-at time, and non-negative nutrient values
- **THEN** the system creates the meal entry attributed to that user and returns the created meal

#### Scenario: Missing required field rejected
- **WHEN** an authenticated user submits a meal missing its name, meal type, or eaten-at time
- **THEN** the system rejects the request with an error identifying the missing field, and no meal is created

#### Scenario: Negative nutrient value rejected
- **WHEN** an authenticated user submits a meal with a negative calorie, protein, carbohydrate, or fat value
- **THEN** the system rejects the request with an error identifying the invalid field, and no meal is created

#### Scenario: Unauthenticated request rejected
- **WHEN** an unauthenticated request attempts to create a meal
- **THEN** the system rejects the request as unauthenticated, and no meal is created

### Requirement: User can list their own logged meals
The system SHALL allow an authenticated user to retrieve a list of meals they have logged, ordered by eaten-at time (most recent first), returning only meals that belong to that user.

#### Scenario: Listing meals returns only the caller's meals
- **WHEN** an authenticated user requests their meal list
- **THEN** the system returns only meal entries created by that user, ordered with the most recently eaten meal first

#### Scenario: Listing meals with no entries
- **WHEN** an authenticated user with no logged meals requests their meal list
- **THEN** the system returns an empty list rather than an error

### Requirement: User can view a single logged meal
The system SHALL allow an authenticated user to retrieve a single meal by identifier if and only if they own it.

#### Scenario: Viewing an owned meal
- **WHEN** an authenticated user requests a meal they own by its identifier
- **THEN** the system returns that meal's full details

#### Scenario: Viewing another user's meal is rejected
- **WHEN** an authenticated user requests a meal owned by a different user
- **THEN** the system rejects the request (not found or forbidden) and does not return the other user's meal data

### Requirement: User can update their own logged meal
The system SHALL allow an authenticated user to modify the name, description, meal type, eaten-at time, or nutrient values of a meal they own, applying the same validation as meal creation, and SHALL reject updates to meals the user does not own.

#### Scenario: Successful update
- **WHEN** an authenticated user submits valid changes to a meal they own
- **THEN** the system updates the meal and returns the updated meal

#### Scenario: Update with invalid data rejected
- **WHEN** an authenticated user submits an update with a missing required field or a negative nutrient value
- **THEN** the system rejects the update with an error identifying the invalid field, and the meal remains unchanged

#### Scenario: Updating another user's meal is rejected
- **WHEN** an authenticated user attempts to update a meal owned by a different user
- **THEN** the system rejects the request and the other user's meal remains unchanged

### Requirement: User can delete their own logged meal
The system SHALL allow an authenticated user to delete a meal they own, and SHALL reject deletion of meals the user does not own.

#### Scenario: Successful deletion
- **WHEN** an authenticated user deletes a meal they own
- **THEN** the system removes the meal, and it no longer appears in that user's meal list

#### Scenario: Deleting another user's meal is rejected
- **WHEN** an authenticated user attempts to delete a meal owned by a different user
- **THEN** the system rejects the request and the other user's meal is not deleted
