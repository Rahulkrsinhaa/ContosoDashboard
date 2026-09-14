# Document Management Contract

## Overview

This contract describes the document feature’s core interactions as a service contract for the existing Blazor Server application. The expected interface is intentionally consistent with the project’s service-first pattern and can be used by UI pages and future API integrations without changing the offline-core design.

## Core interfaces

### IFileStorageService

```csharp
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string storagePath);
    Task DeleteAsync(string storagePath);
    Task<Stream> DownloadAsync(string storagePath);
    Task<string> GetPublicUrlAsync(string storagePath, TimeSpan expiration);
}
```

#### Contract behavior
- `UploadAsync` stores a file and returns a safe, storage-managed path.
- `DeleteAsync` removes the physical file and is expected to fail clearly if the file is missing.
- `DownloadAsync` returns a stream safe for the caller to read.
- `GetPublicUrlAsync` is optional for future server-side generation and migration use.

### DocumentService

```csharp
public interface IDocumentService
{
    Task<Document> UploadAsync(DocumentUploadRequest request, int requestingUserId);
    Task<Document?> GetByIdAsync(int documentId, int requestingUserId);
    Task<List<Document>> GetUserDocumentsAsync(int userId, int requestingUserId);
    Task<List<Document>> GetProjectDocumentsAsync(int projectId, int requestingUserId);
    Task<bool> UpdateMetadataAsync(int documentId, DocumentUpdateRequest request, int requestingUserId);
    Task<bool> ReplaceFileAsync(int documentId, Stream fileStream, string fileName, string contentType, int requestingUserId);
    Task<bool> DeleteAsync(int documentId, int requestingUserId);
    Task<List<Document>> SearchAsync(string query, int requestingUserId);
    Task<bool> ShareAsync(int documentId, DocumentShareRequest request, int requestingUserId);
}
```

## Request payloads

### DocumentUploadRequest

```json
{
  "title": "Quarterly Review",
  "description": "Summary for engineering review",
  "category": "Reports",
  "projectId": 1,
  "tags": ["quarterly", "review"],
  "fileName": "quarterly-review.pdf",
  "contentType": "application/pdf",
  "fileStream": "binary-content"
}
```

### DocumentShareRequest

```json
{
  "recipientUserId": 3,
  "sharedWithProjectId": null,
  "reason": "Review collaboration"
}
```

## Response expectations

- Upload results include a persisted `Document` record with the generated storage path and metadata.
- Search results are scoped to the requesting user’s authorized documents.
- Share operations trigger notification creation for recipients.
- Delete and replace operations require the authenticated user to be authorized and must record the action in the audit log.

## Security requirements

- All read, write, edit, and delete operations must check authorization before acting.
- Physical file access must be performed using the storage service, not directly by the UI or page layer.
- Generated storage paths must use GUID-based names and must not trust the original file name.
- Download and preview endpoints must enforce the same permissions as the service contract.
