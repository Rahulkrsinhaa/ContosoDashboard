namespace ContosoDashboard.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRoot;

    public LocalFileStorageService()
    {
        var root = Path.Combine(AppContext.BaseDirectory, "App_Data", "Documents");
        Directory.CreateDirectory(root);
        _storageRoot = root;
    }

    public string GetStorageRoot() => _storageRoot;

    public async Task<string> SaveAsync(Stream source, string originalFileName, string contentType)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var sanitizedFileName = Path.GetFileName(originalFileName);
        if (string.IsNullOrWhiteSpace(sanitizedFileName))
        {
            sanitizedFileName = $"document-{Guid.NewGuid():N}.bin";
        }

        var safeName = Path.GetFileNameWithoutExtension(sanitizedFileName);
        var extension = Path.GetExtension(sanitizedFileName);
        var storageName = $"{safeName}-{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_storageRoot, storageName);

        await using var outputStream = File.Create(fullPath);
        source.Position = 0;
        await source.CopyToAsync(outputStream);

        var relativePath = Path.Combine("App_Data", "Documents", storageName).Replace('\\', '/');
        return relativePath;
    }

    public async Task DeleteAsync(string storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
        {
            return;
        }

        var filePath = ResolveStoragePath(storedPath);

        if (File.Exists(filePath))
        {
            await Task.Run(() => File.Delete(filePath));
        }
    }

    public Task<Stream> OpenReadAsync(string storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
        {
            throw new InvalidOperationException("Document path is missing.");
        }

        var resolvedPath = ResolveStoragePath(storedPath);

        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException("The stored document was not found.", resolvedPath);
        }

        return Task.FromResult<Stream>(File.OpenRead(resolvedPath));
    }

    private string ResolveStoragePath(string storedPath)
    {
        var normalizedRoot = Path.GetFullPath(_storageRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var candidate = Path.IsPathRooted(storedPath)
            ? Path.GetFullPath(storedPath)
            : Path.GetFullPath(Path.Combine(_storageRoot, Path.GetFileName(storedPath)));

        if (!candidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The stored document path is outside the document storage root.");
        }

        return candidate;
    }
}
