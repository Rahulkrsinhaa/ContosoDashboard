# Research: Document Upload and Management

## Decision: Secure local file storage with a service abstraction

The feature will store uploaded files outside the web root in a dedicated local directory and keep the metadata in the existing EF Core database. A storage interface will abstract the local implementation so a future Azure blob-backed service can be swapped without altering the business logic or UI.

### Rationale

- The project constitution requires offline-first operation and secure file handling.
- The existing project already follows service-layer authorization patterns and uses EF Core for persistence.
- The stakeholder brief explicitly calls for local filesystem storage and an `IFileStorageService` abstraction.

### Alternatives considered

- Storing files under `wwwroot`: rejected because the requirement explicitly prohibits web-accessible storage and requires authorization around downloads.
- Using a single database blob column: rejected because it conflicts with the current repository pattern and future migration guidance.
- Direct file handling in the Razor page layer: rejected because it would bypass service validation and duplicate authorization logic.

## Decision: Project member read access with uploader-managed ownership

Project documents will be readable by project members, while edit and delete rights remain with the original uploader in the default role model. This matches the clarified feature decision and keeps the permission model simple and auditable.

### Rationale

- It aligns with the business need to share project files while preventing uncontrolled modification by all project members.
- It preserves a clear distinction between shared read access and ownership responsibility.

### Alternatives considered

- Project managers controlling all edits: rejected because the stakeholder input and clarification prioritized uploader ownership over project-wide management.
- Any project member can manage any document: rejected because it weakens governance and creates accidental file deletion risks.

## Decision: Replacement keeps the same record and audit trail

A document replacement will update the file behind the same integer `DocumentId` and preserve the metadata record. The system will keep the original audit trail and update the file content, rather than creating a separate version row.

### Rationale

- The feature specification requires preservation of record identity and audit continuity.
- This keeps the data model simpler for a training application and matches the current integer-key pattern.

### Alternatives considered

- New version rows per replacement: rejected because it adds model complexity not required by the feature and is harder to explain within the training app.
- Deleting the old record and creating a new one: rejected because it breaks the intended audit and metadata continuity.

## Decision: Use existing service pattern for authorization and notifications

Document workflows will be implemented through a dedicated `DocumentService` that validates file content, authorizes access, writes metadata, delegates storage to `IFileStorageService`, and raises notifications through the existing notification infrastructure.

### Rationale

- The project already centralizes business rules in services and uses notifications for task and project actions.
- Centralizing document rules preserves the current architecture and keeps authorization logic out of pages.

### Alternatives considered

- Page-only validation: rejected because it would duplicate authorization and weaken the service boundary.
- Controller-first design for all document actions: rejected because this repository uses service-based patterns in Blazor and task/project features.

## Open implementation guidance

- File naming should use GUIDs before database persistence, as required by the stakeholder brief.
- Document categories should be stored as text values instead of enums to keep the implementation simple and aligned with the existing requirements.
- Document access checks should be enforced in service methods before file reads, writes, or deletion actions, not only in the UI.
- The audit trail should record upload, download, share, edit, and delete actions for reporting and security review.
