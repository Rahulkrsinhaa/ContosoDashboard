namespace ContosoDashboard.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream source, string originalFileName, string contentType);
    Task DeleteAsync(string storedPath);
    Task<Stream> OpenReadAsync(string storedPath);
    string GetStorageRoot();
}
