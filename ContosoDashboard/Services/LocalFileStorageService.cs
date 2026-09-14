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

        var absolutePath = storedPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var filePath = absolutePath.StartsWith(_storageRoot, StringComparison.OrdinalIgnoreCase)
            ? absolutePath
            : Path.Combine(_storageRoot, Path.GetFileName(absolutePath));

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

        var absolutePath = storedPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var resolvedPath = absolutePath.StartsWith(_storageRoot, StringComparison.OrdinalIgnoreCase)
            ? absolutePath
            : Path.Combine(_storageRoot, Path.GetFileName(absolutePath));

        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException("The stored document was not found.", resolvedPath);
        }

        return Task.FromResult<Stream>(File.OpenRead(resolvedPath));
    }
}
