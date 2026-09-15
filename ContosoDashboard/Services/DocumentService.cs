using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public interface IDocumentService
{
    Task<Document> UploadAsync(UploadDocumentRequest request, int userId);
    Task<List<Document>> GetAccessibleDocumentsAsync(int userId, int? projectId = null);
    Task<List<Document>> SearchAsync(int userId, string query);
    Task<Stream> DownloadAsync(int documentId, int userId);
    Task<bool> DeleteAsync(int documentId, int userId);
    Task<bool> ShareAsync(int documentId, int recipientUserId, string? message, int userId);
    Task<bool> RevokeShareAsync(int documentId, int recipientUserId, int userId);
    Task<bool> UpdateMetadataAsync(int documentId, string title, string? description, DocumentCategory category, string? tags, int userId);
    Task<bool> ReplaceFileAsync(int documentId, Stream stream, string fileName, string contentType, int userId);
    Task<List<DocumentAuditEntry>> GetAuditEntriesAsync(int documentId, int userId);
    Task<DocumentActivitySummary> GetActivitySummaryAsync(int userId);
}

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storageService;
    private readonly INotificationService _notificationService;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".png", ".jpg", ".jpeg", ".gif"
    };

    public DocumentService(ApplicationDbContext context, IFileStorageService storageService, INotificationService notificationService)
    {
        _context = context;
        _storageService = storageService;
        _notificationService = notificationService;
    }

    public async Task<Document> UploadAsync(UploadDocumentRequest request, int userId)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Document title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new InvalidOperationException("A file is required.");
        }

        ValidateFile(request.FileName, request.Stream);

        var currentUser = await _context.Users.FindAsync(userId);
        if (currentUser == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (request.ProjectId.HasValue)
        {
            var isProjectMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == request.ProjectId.Value && pm.UserId == userId);

            if (!isProjectMember)
            {
                throw new InvalidOperationException("You do not have access to upload files for this project.");
            }
        }

        var safeStoredPath = await _storageService.SaveAsync(request.Stream, request.FileName, request.ContentType);
        var document = new Document
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            FileName = Path.GetFileName(request.FileName),
            StoredFileName = Path.GetFileName(safeStoredPath),
            FilePath = safeStoredPath,
            ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType,
            FileSizeBytes = request.Stream.Length,
            Category = request.Category,
            Tags = request.Tags,
            UploadedByUserId = userId,
            ProjectId = request.ProjectId,
            UploadedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        _context.DocumentAuditEntries.Add(new DocumentAuditEntry
        {
            DocumentId = document.DocumentId,
            UserId = userId,
            Action = DocumentAuditAction.Uploaded,
            Details = $"Uploaded '{document.FileName}'",
            CreatedDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<List<Document>> GetAccessibleDocumentsAsync(int userId, int? projectId = null)
    {
        var accessibleDocumentIds = await _context.Documents
            .Where(d => d.IsDeleted == false)
            .Where(d =>
                d.UploadedByUserId == userId ||
                d.ProjectId == null ||
                d.Project!.ProjectMembers.Any(pm => pm.UserId == userId) ||
                d.Shares.Any(s => s.SharedWithUserId == userId && s.IsActive))
            .Select(d => d.DocumentId)
            .Distinct()
            .ToListAsync();

        var query = _context.Documents
            .Include(d => d.UploadedByUser)
            .Include(d => d.Project)
            .Where(d => accessibleDocumentIds.Contains(d.DocumentId));

        if (projectId.HasValue)
        {
            query = query.Where(d => d.ProjectId == projectId.Value || d.UploadedByUserId == userId && d.ProjectId == null);
        }

        return await query
            .OrderByDescending(d => d.UploadedDate)
            .ToListAsync();
    }

    public async Task<List<Document>> SearchAsync(int userId, string query)
    {
        var trimmed = query?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return await GetAccessibleDocumentsAsync(userId);
        }

        var lower = trimmed.ToLower();

        return await _context.Documents
            .Include(d => d.UploadedByUser)
            .Include(d => d.Project)
            .Where(d => !d.IsDeleted)
            .Where(d =>
                d.UploadedByUserId == userId ||
                d.ProjectId == null ||
                d.Project!.ProjectMembers.Any(pm => pm.UserId == userId) ||
                d.Shares.Any(s => s.SharedWithUserId == userId && s.IsActive))
            .Where(d =>
                d.Title.ToLower().Contains(lower) ||
                (d.Description != null && d.Description.ToLower().Contains(lower)) ||
                (d.Tags != null && d.Tags.ToLower().Contains(lower)) ||
                d.FileName.ToLower().Contains(lower) ||
                d.UploadedByUser.DisplayName.ToLower().Contains(lower))
            .OrderByDescending(d => d.UploadedDate)
            .ToListAsync();
    }

    public async Task<Stream> DownloadAsync(int documentId, int userId)
    {
        var document = await _context.Documents
            .Include(d => d.Project)
            .ThenInclude(p => p!.ProjectMembers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

        if (document == null)
        {
            throw new InvalidOperationException("Document not found.");
        }

        var hasAccess = document.UploadedByUserId == userId
            || document.ProjectId == null && document.UploadedByUserId == userId
            || document.Project != null && document.Project.ProjectMembers.Any(pm => pm.UserId == userId)
            || document.Shares.Any(s => s.SharedWithUserId == userId && s.IsActive);

        if (!hasAccess)
        {
            _context.DocumentAuditEntries.Add(new DocumentAuditEntry
            {
                DocumentId = documentId,
                UserId = userId,
                Action = DocumentAuditAction.AccessDenied,
                Details = "Download attempt denied.",
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            throw new UnauthorizedAccessException("You are not permitted to access this document.");
        }

        try
        {
            var stream = await _storageService.OpenReadAsync(document.FilePath);
            _context.DocumentAuditEntries.Add(new DocumentAuditEntry
            {
                DocumentId = document.DocumentId,
                UserId = userId,
                Action = DocumentAuditAction.Downloaded,
                Details = $"Downloaded '{document.FileName}'",
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return stream;
        }
        catch (FileNotFoundException)
        {
            throw new InvalidOperationException("The document file is missing from storage.");
        }
    }

    public async Task<bool> DeleteAsync(int documentId, int userId)
    {
        var document = await _context.Documents
            .Include(d => d.Project)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

        if (document == null)
        {
            return false;
        }

        if (document.UploadedByUserId != userId)
        {
            return false;
        }

        document.IsDeleted = true;
        document.UpdatedDate = DateTime.UtcNow;
        _context.DocumentAuditEntries.Add(new DocumentAuditEntry
        {
            DocumentId = documentId,
            UserId = userId,
            Action = DocumentAuditAction.Deleted,
            Details = $"Deleted '{document.FileName}'",
            CreatedDate = DateTime.UtcNow
        });

        await _storageService.DeleteAsync(document.FilePath);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ShareAsync(int documentId, int recipientUserId, string? message, int userId)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);
        if (document == null || document.UploadedByUserId != userId || userId == recipientUserId)
        {
            return false;
        }

        var recipientExists = await _context.Users.AnyAsync(u => u.UserId == recipientUserId);
        if (!recipientExists)
        {
            return false;
        }

        var share = await _context.DocumentShares
            .FirstOrDefaultAsync(s => s.DocumentId == documentId && s.SharedWithUserId == recipientUserId);

        if (share == null)
        {
            share = new DocumentShare
            {
                DocumentId = documentId,
                SharedWithUserId = recipientUserId,
                SharedByUserId = userId,
                Message = message,
                SharedDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.DocumentShares.Add(share);
        }
        else
        {
            share.IsActive = true;
            share.Message = message;
            share.SharedDate = DateTime.UtcNow;
        }

        _context.DocumentAuditEntries.Add(new DocumentAuditEntry
        {
            DocumentId = documentId,
            UserId = userId,
            Action = DocumentAuditAction.Shared,
            Details = $"Shared '{document.FileName}' with user {recipientUserId}.",
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(new Notification
        {
            UserId = recipientUserId,
            Title = "Document shared with you",
            Message = $"{document.Title} is now available in your shared documents.",
            Type = NotificationType.DocumentShared,
            Priority = NotificationPriority.Informational
        });

        return true;
    }

    public async Task<bool> RevokeShareAsync(int documentId, int recipientUserId, int userId)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);
        if (document == null || document.UploadedByUserId != userId)
        {
            return false;
        }

        var share = await _context.DocumentShares
            .FirstOrDefaultAsync(s => s.DocumentId == documentId && s.SharedWithUserId == recipientUserId && s.IsActive);
        if (share == null)
        {
            return false;
        }

        share.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateMetadataAsync(int documentId, string title, string? description, DocumentCategory category, string? tags, int userId)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);
        if (document == null || document.UploadedByUserId != userId || string.IsNullOrWhiteSpace(title))
        {
            return false;
        }

        document.Title = title.Trim();
        document.Description = description;
        document.Category = category;
        document.Tags = tags;
        document.UpdatedDate = DateTime.UtcNow;
        _context.DocumentAuditEntries.Add(new DocumentAuditEntry
        {
            DocumentId = documentId,
            UserId = userId,
            Action = DocumentAuditAction.Updated,
            Details = $"Updated metadata for '{document.FileName}'.",
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReplaceFileAsync(int documentId, Stream stream, string fileName, string contentType, int userId)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);
        if (document == null || document.UploadedByUserId != userId)
        {
            return false;
        }

        ValidateFile(fileName, stream);
        var oldPath = document.FilePath;
        var newPath = await _storageService.SaveAsync(stream, fileName, contentType);
        document.FileName = Path.GetFileName(fileName);
        document.StoredFileName = Path.GetFileName(newPath);
        document.FilePath = newPath;
        document.ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
        document.FileSizeBytes = stream.Length;
        document.UpdatedDate = DateTime.UtcNow;
        _context.DocumentAuditEntries.Add(new DocumentAuditEntry
        {
            DocumentId = documentId,
            UserId = userId,
            Action = DocumentAuditAction.Replaced,
            Details = $"Replaced '{document.FileName}'.",
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        await _storageService.DeleteAsync(oldPath);
        return true;
    }

    public async Task<List<DocumentAuditEntry>> GetAuditEntriesAsync(int documentId, int userId)
    {
        var document = await _context.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);
        if (document == null || !await CanManageOrViewAsync(document, userId))
        {
            return new List<DocumentAuditEntry>();
        }

        return await _context.DocumentAuditEntries
            .Include(a => a.User)
            .Where(a => a.DocumentId == documentId)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }

    public async Task<DocumentActivitySummary> GetActivitySummaryAsync(int userId)
    {
        var canReport = await _context.Users.AnyAsync(u => u.UserId == userId &&
            (u.Role == UserRole.Administrator || u.Role == UserRole.ProjectManager));
        if (!canReport)
        {
            throw new UnauthorizedAccessException("You are not permitted to view document activity reports.");
        }

        var entries = _context.DocumentAuditEntries.AsNoTracking();
        return new DocumentActivitySummary
        {
            TotalActions = await entries.CountAsync(),
            Uploads = await entries.CountAsync(e => e.Action == DocumentAuditAction.Uploaded),
            Downloads = await entries.CountAsync(e => e.Action == DocumentAuditAction.Downloaded),
            Shares = await entries.CountAsync(e => e.Action == DocumentAuditAction.Shared),
            Deletes = await entries.CountAsync(e => e.Action == DocumentAuditAction.Deleted),
            CategoryCounts = await _context.Documents.AsNoTracking().Where(d => !d.IsDeleted)
                .GroupBy(d => d.Category)
                .Select(g => new DocumentCategoryCount { Category = g.Key, Count = g.Count() })
                .ToListAsync()
        };
    }

    private async Task<bool> CanManageOrViewAsync(Document document, int userId)
    {
        return document.UploadedByUserId == userId
            || document.ProjectId == null && document.UploadedByUserId == userId
            || document.ProjectId.HasValue && await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == document.ProjectId && pm.UserId == userId)
            || await _context.DocumentShares.AnyAsync(s => s.DocumentId == document.DocumentId && s.SharedWithUserId == userId && s.IsActive);
    }

    private void ValidateFile(string fileName, Stream stream)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type. Allowed formats: PDF, Word, Excel, PowerPoint, text, and images.");
        }
        if (stream is null || stream.Length == 0 || stream.Length > 25 * 1024 * 1024)
        {
            throw new InvalidOperationException("The selected file must be greater than zero and no larger than 25 MB.");
        }
    }
}

public class DocumentActivitySummary
{
    public int TotalActions { get; set; }
    public int Uploads { get; set; }
    public int Downloads { get; set; }
    public int Shares { get; set; }
    public int Deletes { get; set; }
    public List<DocumentCategoryCount> CategoryCounts { get; set; } = new();
}

public class DocumentCategoryCount
{
    public DocumentCategory Category { get; set; }
    public int Count { get; set; }
}

public class UploadDocumentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DocumentCategory Category { get; set; } = DocumentCategory.Other;
    public string ContentType { get; set; } = "application/octet-stream";
    public Stream Stream { get; set; } = Stream.Null;
    public int? ProjectId { get; set; }
    public string? Tags { get; set; }
}
