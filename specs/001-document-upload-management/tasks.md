# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-upload-management/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish the document storage and service structure needed across the feature.

- [X] T001 Create the feature folder structure and document storage directory under ContosoDashboard/Services/ and ContosoDashboard/Models/
- [X] T002 [P] Add storage and file handling configuration values in ContosoDashboard/appsettings.json and ContosoDashboard/appsettings.Development.json
- [X] T003 [P] Update ContosoDashboard/Program.cs to register the document storage abstraction and related services

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before any user story can be implemented.

- [X] T004 Create the document entity model in ContosoDashboard/Models/Document.cs with required metadata, category, storage path, and audit-friendly validation
- [X] T005 [P] Create the document sharing model in ContosoDashboard/Models/DocumentShare.cs and the audit model in ContosoDashboard/Models/DocumentAuditEntry.cs
- [X] T006 [P] Update ContosoDashboard/Data/ApplicationDbContext.cs to register Document, DocumentShare, and DocumentAuditEntry DbSets and application indexes
- [X] T007 Implement the storage abstraction in ContosoDashboard/Services/IFileStorageService.cs and the local filesystem implementation in ContosoDashboard/Services/LocalFileStorageService.cs
- [X] T008 Implement the service contract and core document validation in ContosoDashboard/Services/DocumentService.cs
- [X] T009 Add document notification integration points in ContosoDashboard/Services/NotificationService.cs and ContosoDashboard/Services/DashboardService.cs

**Checkpoint**: Foundation ready - user story implementation can begin in parallel.

---

## Phase 3: User Story 1 - Upload, organize, and access project and personal document files (Priority: P1) 🎯 MVP

**Goal**: Let users upload valid documents, see them in their personal/project views, and search or filter them by authorized metadata.

**Independent Test**: A logged-in user can upload a valid file, assign metadata, and find it in the correct personal or project document list without seeing unauthorized records.

### Implementation for User Story 1

- [X] T010 [P] [US1] Add upload and replace validation logic in ContosoDashboard/Services/DocumentService.cs
- [X] T011 [US1] Add user/project authorization checks for document read, edit, and delete operations in ContosoDashboard/Services/DocumentService.cs
- [X] T012 [P] [US1] Build the document dashboard and browsing page in ContosoDashboard/Pages/Documents.razor with upload, search, filter, and list views
- [X] T013 [US1] Integrate recent-documents and summary counts in ContosoDashboard/Pages/Index.razor and ContosoDashboard/Services/DashboardService.cs
- [X] T014 [P] [US1] Add project document rendering in ContosoDashboard/Pages/ProjectDetails.razor so project members can view and download related files
- [X] T015 [US1] Add replacement/edit metadata flow for uploaded documents in ContosoDashboard/Pages/Documents.razor and ContosoDashboard/Services/DocumentService.cs

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently.

---

## Phase 4: User Story 2 - Share document access with the correct users and teams (Priority: P2)

**Goal**: Enable users to share documents with specific recipients and have the access model reflected in the shared documents experience.

**Independent Test**: A user can share a document with another user and confirm the recipient sees the file in the shared area and receives a notification.

### Implementation for User Story 2

- [ ] T016 [P] [US2] Add sharing and recipient logic to ContosoDashboard/Models/DocumentShare.cs and the document service layer in ContosoDashboard/Services/DocumentService.cs
- [ ] T017 [US2] Implement notification creation for document shares in ContosoDashboard/Services/NotificationService.cs
- [ ] T018 [P] [US2] Add shared-document views and recipient access handling in ContosoDashboard/Pages/Documents.razor
- [ ] T019 [US2] Update project/task context so related documents are viewable from ContosoDashboard/Pages/Tasks.razor and ContosoDashboard/Pages/ProjectDetails.razor
- [ ] T020 [US2] Validate denied access behavior for unauthorized preview/download attempts in ContosoDashboard/Services/DocumentService.cs

**Checkpoint**: At this point, User Stories 1 and 2 should both work independently.

---

## Phase 5: User Story 3 - Keep document activity transparent and reportable (Priority: P3)

**Goal**: Record document actions for audit and administrator reporting without breaking the user-facing document experience.

**Independent Test**: An administrator can review upload, download, edit, share, and delete actions to identify document usage patterns and historical changes.

### Implementation for User Story 3

- [ ] T021 [P] [US3] Add audit logging model and persistence in ContosoDashboard/Models/DocumentAuditEntry.cs and ContosoDashboard/Data/ApplicationDbContext.cs
- [ ] T022 [US3] Implement activity recording for upload, download, share, edit, and delete operations in ContosoDashboard/Services/DocumentService.cs
- [ ] T023 [P] [US3] Add the document reporting and audit summary experience in ContosoDashboard/Pages/Documents.razor or ContosoDashboard/Pages/Reports.razor
- [ ] T024 [US3] Add aggregation logic for uploader activity, file-category trends, and access patterns in ContosoDashboard/Services/DocumentService.cs
- [ ] T025 [US3] Confirm the feature aligns with the approved quickstart validation in ContosoDashboard/specs/001-document-upload-management/quickstart.md

**Checkpoint**: All user stories should now be independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final hardening across the full feature.

- [ ] T026 [P] Perform access-control review and regression validation across ContosoDashboard/Services/DocumentService.cs, ContosoDashboard/Services/ProjectService.cs, and ContosoDashboard/Services/TaskService.cs
- [ ] T027 [P] Review and tighten validation for file type, size, and user-controlled naming in ContosoDashboard/Services/LocalFileStorageService.cs and ContosoDashboard/Services/DocumentService.cs
- [ ] T028 Update training documentation and usage notes in README.md and ContosoDashboard/specs/001-document-upload-management/quickstart.md
- [ ] T029 run a full smoke test against the document workflow and confirm the repository remains consistent with the specification in ContosoDashboard/specs/001-document-upload-management/spec.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion
- **User Story 2 (Phase 4)**: Depends on Foundational completion and may integrate with US1 but should be independently testable
- **User Story 3 (Phase 5)**: Depends on Foundational completion and may integrate with US1/US2 but should be independently testable
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start immediately after Phase 2; it is the MVP and independent from US2/US3
- **User Story 2 (P2)**: Can begin after Phase 2; it reinforces the sharing and collaborative flow
- **User Story 3 (P3)**: Can begin after Phase 2; it is an audit/reporting extension of the same foundation

### Parallel Opportunities

- Phase 1 tasks can run in parallel if multiple files are being prepared
- Phase 2 tasks marked [P] can run in parallel
- Once the foundational phase is complete, the three user story phases can be implemented in parallel by separate contributors
- Within each story, tasks marked [P] can be worked in parallel when file ownership is separated

---

## Parallel Example: User Story 1

```bash
# Example parallel work for User Story 1:
# 1) upload validation logic in ContosoDashboard/Services/DocumentService.cs
# 2) document listing/search UI in ContosoDashboard/Pages/Documents.razor
# 3) recent document widget updates in ContosoDashboard/Pages/Index.razor
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Stop and validate the upload/list/search flow independently before moving on

### Incremental Delivery

1. Setup + Foundational → foundation ready
2. User Story 1 → validate upload and browse flow
3. User Story 2 → validate sharing and notifications
4. User Story 3 → validate auditing/reporting
5. Final polish and regression review

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once the foundation is in place:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. End with cross-cutting polish and validation
