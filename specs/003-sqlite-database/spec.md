# Feature Specification: SQLite Database Connectivity

**Feature Branch**: `003-sqlite-database`  
**Created**: 2026-09-15  
**Status**: Draft  
**Input**: User description: "connect it o sql lite"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Run the dashboard without SQL Server (Priority: P1)

As a learner or developer, I want the dashboard to use a local embedded database so that I can run the application without installing or configuring SQL Server LocalDB.

**Why this priority**: Removing the unavailable LocalDB dependency is required for the application to start and for all database-backed pages to be usable in the training environment.

**Independent Test**: On a machine without SQL Server or LocalDB installed, start the application and confirm that it creates or opens its local database and serves the login page without a database connection error.

**Acceptance Scenarios**:

1. **Given** a machine without SQL Server or LocalDB, **When** the application starts, **Then** database initialization completes without a SQL Server connection error.
2. **Given** the local database does not yet exist, **When** the application starts, **Then** the application creates the database and required tables automatically.
3. **Given** the local database has been initialized, **When** a user opens the login page, **Then** the page loads successfully and can read the available users.

---

### User Story 2 - Preserve existing dashboard data behavior (Priority: P2)

As a dashboard user, I want existing seeded users, projects, tasks, notifications, and document records to remain available through the local database so that changing database providers does not change the training workflow.

**Why this priority**: The provider migration is valuable only if the application’s existing user journeys continue to work with the same data relationships and access rules.

**Independent Test**: Start with a newly created local database, sign in using a seeded training user, and verify that dashboard data can be read and related records remain connected.

**Acceptance Scenarios**:

1. **Given** a newly initialized local database, **When** the application loads seed data, **Then** the existing training users and sample project data are available.
2. **Given** a signed-in user with access to dashboard data, **When** they open projects or tasks, **Then** records load without provider-specific connection errors.
3. **Given** a user creates or updates supported dashboard data, **When** they revisit the relevant page, **Then** the change is persisted in the local database.

---

### User Story 3 - Keep local setup clear and repeatable (Priority: P3)

As a contributor, I want the local database location and startup behavior to be documented so that I can reset or reproduce the training environment consistently.

**Why this priority**: Clear local setup prevents contributors from assuming a separate database server is required and makes troubleshooting predictable.

**Independent Test**: Follow the documented setup instructions on a clean machine, start the application, and identify the created database file and reset procedure.

**Acceptance Scenarios**:

1. **Given** a contributor reads the project setup documentation, **When** they follow the database instructions, **Then** they know the application uses a local embedded database and do not need SQL Server LocalDB.
2. **Given** a contributor removes the local database file, **When** they restart the application, **Then** the database is recreated with the expected training data.
3. **Given** database initialization cannot complete, **When** the application starts, **Then** the failure is logged clearly and does not falsely report successful initialization.

### Edge Cases

- What happens when the local database file is missing at startup?
- What happens when the application cannot create or write to the directory containing the database file?
- What happens when an existing database has an incompatible schema?
- What happens when two application instances attempt to access the local database at the same time?
- What happens when a database-backed page is opened before initialization has completed?
- What happens when the local database is reset and seed data must be recreated?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST use a local embedded SQLite database instead of requiring SQL Server or SQL Server LocalDB for development and training execution.
- **FR-002**: The application MUST obtain its database location from the application configuration rather than hard-coding a machine-specific server name.
- **FR-003**: The application MUST create the configured local database when it does not already exist.
- **FR-004**: The application MUST initialize all tables and relationships required by the existing dashboard features.
- **FR-005**: The application MUST preserve the existing seeded training users, projects, tasks, and related sample data after initialization.
- **FR-006**: The application MUST preserve existing authentication, authorization, and user-scoping behavior when reading or changing database-backed data.
- **FR-007**: The application MUST persist supported create, update, and delete operations across application restarts.
- **FR-008**: The application MUST provide a clear logged error when the database cannot be created, opened, or written.
- **FR-009**: The application MUST not require a separately installed SQL Server, LocalDB runtime, or remote database service to run the training environment.
- **FR-010**: The project documentation MUST identify the local database location, startup initialization behavior, and reset procedure.
- **FR-011**: The project build and automated tests MUST complete successfully with the SQLite configuration.
- **FR-012**: The application MUST avoid exposing the local database file through static web content or user download routes.

### Key Entities *(include if feature involves data)*

- **Local Application Database**: The persistent store for dashboard users, projects, tasks, notifications, documents, sharing records, and audit records.
- **Database Configuration**: The application setting that identifies where the local database is stored and how it is opened.
- **Seed Data Set**: The initial training users and sample records created when the database is initialized for the first time.

## Assumptions

- SQLite is the selected embedded database provider for this training-focused migration.
- Existing entity relationships, seed records, service contracts, authentication behavior, and authorization rules remain in scope and should not be redesigned.
- The local database file is intended for development and training use; production deployment and multi-node database hosting are outside this feature.
- Existing data in the SQL Server LocalDB database is not automatically migrated; a clean local database may be created from the application model and seed data.
- Database schema evolution beyond initial creation is outside this feature unless required to keep the existing application model usable.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new contributor can start the application on a machine without SQL Server or LocalDB installed and reach the login page within 2 minutes, excluding dependency download time.
- **SC-002**: 100% of fresh application starts create or open the local database without a SQL Server provider or LocalDB runtime error.
- **SC-003**: 100% of existing database-backed smoke checks for login, projects, tasks, notifications, and documents pass using the local database.
- **SC-004**: Data created through supported dashboard workflows remains available after the application is stopped and restarted in at least 10 consecutive verification runs.
- **SC-005**: A contributor can identify the database location and reset procedure from the project documentation in under 1 minute.
- **SC-006**: The application exposes zero database files through its static file or user-download paths during verification.
