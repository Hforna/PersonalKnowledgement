namespace PersonalKnowledge.Domain.Services;

public interface IStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string fileKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string fileKey, CancellationToken cancellationToken = default);
}

