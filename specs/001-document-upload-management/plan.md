# Implementation Plan: Document Upload and Management

**Branch**: `001-document-upload-management` | **Date**: 2026-09-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-document-upload-management/spec.md`

## Summary

Add secure document upload, authorized access, sharing, replacement, and audit/reporting workflows to the offline Blazor dashboard. The implementation uses the existing EF Core model and service boundaries, local filesystem storage behind `IFileStorageService`, and SQLite for the local database.

## Technical Context

**Language/Version**: C# on .NET 10.0
**Primary Dependencies**: ASP.NET Core Blazor Server, Entity Framework Core 10, SQLite, xUnit
**Storage**: SQLite database file plus local filesystem storage outside `wwwroot`
**Testing**: `dotnet test` with xUnit and EF Core InMemory fixtures
**Target Platform**: Windows/Linux/macOS development environments with the .NET 10 SDK
**Project Type**: Single ASP.NET Core web application with a separate test project
**Performance Goals**: Responsive document search and dashboard summaries for the training dataset
**Constraints**: Offline-first, mock authentication, 25 MB upload limit, service-layer authorization, no public file paths
**Scale/Scope**: Training dashboard entities and document workflows for a small local dataset

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Security-First for Training**: PASS - document reads, writes, downloads, sharing, and reporting are checked in the service layer.
- **Offline-First, Cloud-Ready Architecture**: PASS - SQLite and local file storage run without external services and remain behind existing abstractions.
- **Testable and Incremental Delivery**: PASS - document access, sharing, replacement, validation, and audit behavior have focused tests.
- **User-Scoped Data Integrity**: PASS - project membership, uploader ownership, and recipient access are enforced before data or files are exposed.
- **Simplicity, Clarity, and Maintainability**: PASS - changes use the existing model/service/page structure without a new repository layer.
- **CI Before Merge**: PASS - build and test commands are documented and pass locally.

## Project Structure

### Documentation (this feature)

```text
specs/001-document-upload-management/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
└── tasks.md
```

### Source Code

```text
ContosoDashboard/
├── Data/ApplicationDbContext.cs
├── Models/Document.cs
├── Models/DocumentShare.cs
├── Models/DocumentAuditEntry.cs
├── Services/DocumentService.cs
├── Services/IFileStorageService.cs
├── Services/LocalFileStorageService.cs
├── Services/NotificationService.cs
├── Pages/Documents.razor
├── Pages/Index.razor
└── Pages/ProjectDetails.razor

ContosoDashboard.Tests/
└── DocumentServiceAccessTests.cs
```

**Structure Decision**: Use the existing single ASP.NET Core web project with model, service, data, and Razor page boundaries. Keep focused document regression tests in the existing xUnit test project.

## Design Decisions

- Keep physical document content outside `wwwroot`; only authorized service methods can open or delete files.
- Generate unique stored filenames and resolve paths beneath the configured storage root.
- Keep uploader-only management and project-member/shared-user read access in `DocumentService`.
- Preserve a document's `DocumentId` during file replacement and append audit entries for every completed action.
- Use SQLite as the local EF Core provider so the training environment does not require SQL Server LocalDB.
- Use EF Core InMemory fixtures for fast service authorization tests while validating the real application with SQLite startup smoke tests.

## Implementation Phases

1. Complete shared model, storage, persistence, and service registration prerequisites.
2. Deliver the P1 upload, browse, search, download, and ownership MVP.
3. Add P2 sharing, notifications, and recipient access.
4. Add P3 replacement, audit history, reporting, and category/activity summaries.
5. Harden file validation and path handling, update training documentation, and run the full build/test/smoke validation.

## Complexity Tracking

No constitution violations require justification for this feature.
