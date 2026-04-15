using PersonalKnowledge.Domain.Services;
using PersonalKnowledge.Domain.Exceptions;

namespace PersonalKnowledge.Infrastructure.Services;

public class LocalFileStorageService : IStorageService
{
    private readonly string _storagePath;

    public LocalFileStorageService(string storagePath)
    {
        _storagePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
        
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileKey = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(_storagePath, fileKey);

            using (var fileToWrite = File.Create(filePath))
            {
                await fileStream.CopyToAsync(fileToWrite, cancellationToken);
            }

            return fileKey;
        }
        catch (Exception ex)
        {
            throw new StorageException(fileName, "Failed to upload file to local storage", ex);
        }
    }

    public async Task<Stream> DownloadAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = Path.Combine(_storagePath, fileKey);

            if (!File.Exists(filePath))
                throw new StorageException(fileKey, "File not found in local storage");

            var stream = File.OpenRead(filePath);
            return await Task.FromResult(stream);
        }
        catch (StorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new StorageException(fileKey, "Failed to download file from local storage", ex);
        }
    }

    public async Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = Path.Combine(_storagePath, fileKey);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new StorageException(fileKey, "Failed to delete file from local storage", ex);
        }
    }

    public async Task<bool> ExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = Path.Combine(_storagePath, fileKey);
            return await Task.FromResult(File.Exists(filePath));
        }
        catch (Exception ex)
        {
            throw new StorageException(fileKey, "Failed to check if file exists in local storage", ex);
        }
    }
}


