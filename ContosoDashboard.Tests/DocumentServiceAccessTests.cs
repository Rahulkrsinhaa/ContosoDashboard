using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContosoDashboard.Tests;

public class DocumentServiceAccessTests
{
    [Fact]
    public async Task UploadAsync_RejectsUnsupportedFileType()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        context.Users.Add(new User
        {
            UserId = 1,
            Email = "test@example.com",
            DisplayName = "Test User",
            Role = UserRole.Employee,
            Department = "Engineering",
            JobTitle = "Engineer"
        });
        await context.SaveChangesAsync();

        var storage = new LocalFileStorageService();
        var service = new DocumentService(context, storage, new NotificationService(context));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadAsync(new UploadDocumentRequest
            {
                FileName = "notes.exe",
                Title = "Unsafe file",
                Category = DocumentCategory.Other,
                ContentType = "application/x-msdownload",
                Stream = new MemoryStream(new byte[] { 1, 2, 3, 4 }),
                ProjectId = null,
                Tags = "test"
            }, 1));
    }

    [Fact]
    public async Task ShareAsync_CreatesNotification_AndRevokeRemovesAccess()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        context.Users.AddRange(
            new User { UserId = 1, Email = "owner@example.com", DisplayName = "Owner", Role = UserRole.Employee },
            new User { UserId = 2, Email = "recipient@example.com", DisplayName = "Recipient", Role = UserRole.Employee });
        context.Documents.Add(new Document
        {
            DocumentId = 10,
            Title = "Plan",
            FileName = "plan.txt",
            StoredFileName = "plan-10.txt",
            FilePath = "App_Data/Documents/plan-10.txt",
            ContentType = "text/plain",
            Category = DocumentCategory.Other,
            UploadedByUserId = 1
        });
        await context.SaveChangesAsync();

        var service = new DocumentService(context, new LocalFileStorageService(), new NotificationService(context));

        Assert.True(await service.ShareAsync(10, 2, "Please review", 1));
        Assert.True(await context.DocumentShares.AnyAsync(s => s.DocumentId == 10 && s.SharedWithUserId == 2 && s.IsActive));
        Assert.True(await context.Notifications.AnyAsync(n => n.UserId == 2 && n.Type == NotificationType.DocumentShared));

        Assert.True(await service.RevokeShareAsync(10, 2, 1));
        Assert.False(await context.DocumentShares.AnyAsync(s => s.DocumentId == 10 && s.SharedWithUserId == 2 && s.IsActive));
    }

    [Fact]
    public async Task ReplaceFileAsync_PreservesDocumentId_AndAddsAuditEntry()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        context.Users.Add(new User { UserId = 1, Email = "owner@example.com", DisplayName = "Owner", Role = UserRole.Employee });
        context.Documents.Add(new Document
        {
            DocumentId = 11,
            Title = "Plan",
            FileName = "plan.txt",
            StoredFileName = "plan-old.txt",
            FilePath = "App_Data/Documents/plan-old.txt",
            ContentType = "text/plain",
            Category = DocumentCategory.Other,
            UploadedByUserId = 1
        });
        await context.SaveChangesAsync();

        var service = new DocumentService(context, new LocalFileStorageService(), new NotificationService(context));
        Assert.True(await service.ReplaceFileAsync(11, new MemoryStream(new byte[] { 1, 2, 3 }), "updated.txt", "text/plain", 1));

        var document = await context.Documents.SingleAsync(d => d.DocumentId == 11);
        Assert.Equal(11, document.DocumentId);
        Assert.Equal("updated.txt", document.FileName);
        Assert.True(await context.DocumentAuditEntries.AnyAsync(a => a.DocumentId == 11 && a.Action == DocumentAuditAction.Replaced));
    }
}
