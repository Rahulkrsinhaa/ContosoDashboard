# Quickstart: Document Upload and Management Validation

## Prerequisites

- .NET 8.0 SDK installed
- Local project dependencies restored
- Application running in a local development environment
- At least two users available in the mock authentication flow, such as an employee and a project manager

## Validation scenarios

### 1. Upload a valid file

1. Start the app with `dotnet run`.
2. Log in as a valid mock user.
3. Navigate to the document management page or project document area.
4. Select a supported file such as PDF or a DOCX file under 25 MB.
5. Enter a title, choose a valid category, and optionally add a project association and tags.
6. Submit the upload.

Expected outcome:
- The file is accepted.
- A document record appears in the user’s document list.
- The file is stored outside the web root and accessible under the configured local storage pattern.

### 2. Reject invalid input

1. Attempt to upload a file larger than 25 MB or with an unsupported extension.
2. Submit the form.

Expected outcome:
- The system shows a clear validation message.
- No document record is created.
- No file is saved to the storage location.

### 3. View and search documents

1. Upload several documents with different titles, categories, and tags.
2. Use the filtering and search controls.

Expected outcome:
- Matching docs appear in the correct filtered list.
- Results only include documents the logged-in user is authorized to access.
- Search results return within the requested time target in normal usage conditions.

### 4. Replace a document file

1. Upload a document.
2. Replace the file with an updated version using the document edit flow.

Expected outcome:
- The same document record remains active.
- The file content updates without losing the record identity or audit history.

### 5. Share a document

1. Upload a document as a user with a document to share.
2. Share it with another user or project group.
3. Log in as the recipient.

Expected outcome:
- The recipient receives an in-app notification.
- The document appears in the recipient’s accessible list.
- The recipient can preview or download it if permitted.

### 6. Confirm access control

1. Log in as a project member who is not the uploader.
2. Attempt to view, download, edit, or delete a project document.

Expected outcome:
- Read and download access succeeds for authorized project members.
- Edit or delete access is blocked unless the user is the uploader or is otherwise explicitly authorized by the project exception policy.

### 7. Audit activity

1. Perform upload, edit, share, and delete actions.
2. Open the audit or reporting area.

Expected outcome:
- Document activity is recorded in a visible report or log.
- Uploaders and actions are traceable for review.
