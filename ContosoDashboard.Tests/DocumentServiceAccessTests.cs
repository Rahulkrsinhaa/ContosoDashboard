using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.EntityFrameworkCore;

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
        var service = new DocumentService(context, storage);

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
}

public class UploadDocumentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DocumentCategory Category { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public MemoryStream Stream { get; set; } = new();
    public int? ProjectId { get; set; }
    public string? Tags { get; set; }
}
