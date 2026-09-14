# Data Model: Document Upload and Management

## Overview

The feature extends the existing ContosoDashboard domain model with document metadata and sharing records while keeping the overall structure aligned with the current EF Core design. The main entities are `Document`, `DocumentShare`, and `DocumentAuditEntry`, with relationships to `User` and `Project`.

## Entities

### Document

Represents an uploaded file associated with a user and optionally a project.

| Field | Type | Notes |
| --- | --- | --- |
| DocumentId | int | Primary key; integer key matches the current project/user pattern |
| Title | string | Required; user-visible display name |
| Description | string? | Optional summary or notes |
| Category | string | Required; values such as Project Documents, Team Resources, Personal Files, Reports, Presentations, Other |
| FileName | string | System-managed name; not user-controlled |
| StoredFilePath | string | Relative or internal path used by the file storage service |
| FileType | string | MIME type; allow long values up to 255 characters |
| FileSizeBytes | long | Size of the uploaded file |
| UploadedByUserId | int | Foreign key to `User` |
| ProjectId | int? | Optional association to a project |
| UploadedDateUtc | DateTime | Timestamp captured at upload |
| UpdatedDateUtc | DateTime | Last modification timestamp |
| IsActive | bool | Soft state for active versus deleted records |
| Tags | string? | Optional tag list, stored as a simple text payload for training simplicity |

Relationships:
- Many documents belong to one uploader (`User`)
- Many documents may belong to zero or one project (`Project`)
- A document can have many share records and many audit entries

### DocumentShare

Represents a user-level or project-level sharing relationship.

| Field | Type | Notes |
| --- | --- | --- |
| DocumentShareId | int | Primary key |
| DocumentId | int | Foreign key to `Document` |
| SharedByUserId | int | User who initiated the share |
| RecipientUserId | int? | Optional direct recipient |
| SharedWithProjectId | int? | Optional project audience share |
| SharedDateUtc | DateTime | Share timestamp |
| ShareReason | string? | Optional note or context |
| IsActive | bool | Indicates whether access remains active |

Relationships:
- Many shares are associated with one document
- One user shares with one or many recipients
- A share can target a user or a project audience, but not both

### DocumentAuditEntry

Tracks activity for reporting and review.

| Field | Type | Notes |
| --- | --- | --- |
| DocumentAuditEntryId | int | Primary key |
| DocumentId | int | Foreign key to `Document` |
| UserId | int | User who performed the action |
| ActionType | string | Upload, Download, Edit, Delete, Share |
| ActionDateUtc | DateTime | Timestamp for the event |
| Details | string? | Additional context for the action |

Relationships:
- Many audit entries correspond to one document
- Many audit entries correspond to one user

## Validation Rules

- `Title` is required and must not be blank.
- `Category` is required and must match one of the approved category labels.
- `FileSizeBytes` must be positive and not exceed the 25 MB limit at the application layer.
- `FileType` should be a non-empty MIME type string.
- `StoredFilePath` must be generated before database persistence and must not use user-supplied file names directly.
- `ProjectId` is optional, but when present it must reference an existing project.
- A share must target either a specific user or a project audience, not both.

## State Transitions

### Document lifecycle
- Uploaded → Active
- Active → Updated during replacement
- Active → Deleted (soft or hard delete policy as chosen by implementation)
- Deleted → Not visible in standard views

### Share lifecycle
- Created → Active
- Active → Revoked or expired
- Revoked → Inactive

## Notes for implementation

- The current feature should keep the data model intentionally simple and use text-based categories instead of enumerated values.
- The file storage abstraction should be the only place responsible for physical file access operations.
- Document services should store one metadata record while replacing the underlying file content, preserving the original record ID for continuity.
