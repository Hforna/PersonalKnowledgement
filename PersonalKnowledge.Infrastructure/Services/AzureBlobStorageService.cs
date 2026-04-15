using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PersonalKnowledge.Domain.Services;
using PersonalKnowledge.Domain.Exceptions;

namespace PersonalKnowledge.Infrastructure.Services;

public class AzureBlobStorageService : IStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(BlobContainerClient containerClient)
    {
        _containerClient = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobName = $"{Guid.NewGuid()}_{fileName}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(fileStream, overwrite: true, cancellationToken: cancellationToken);

            return blobName;
        }
        catch (Exception ex)
        {
            throw new StorageException(fileName, $"Failed to upload file to blob storage", ex);
        }
    }

    public async Task<Stream> DownloadAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileKey);
            var download = await blobClient.DownloadAsync(cancellationToken: cancellationToken);
            return download.Value.Content;
        }
        catch (Exception ex)
        {
            throw new StorageException(fileKey, "Failed to download file from blob storage", ex);
        }
    }

    public async Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileKey);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new StorageException(fileKey, "Failed to delete file from blob storage", ex);
        }
    }

    public async Task<bool> ExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileKey);
            var exists = await blobClient.ExistsAsync(cancellationToken: cancellationToken);
            return exists.Value;
        }
        catch (Exception ex)
        {
            throw new StorageException(fileKey, "Failed to check if file exists in blob storage", ex);
        }
    }
}

