using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using PersonalKnowledge.Domain.Services;
using PersonalKnowledge.Domain.Exceptions;
using PersonalKnowledge.Domain.Constants;

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
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
            
            var sanitizedFileName = fileName.Replace(" ", "_");
            var blobName = $"{Guid.NewGuid()}_{sanitizedFileName}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            var extension = Path.GetExtension(fileName);
            var fileExtension = FileTypeIdentifiers.GetFileExtension(extension);
            var contentType = fileExtension.HasValue ? FileTypeIdentifiers.GetMimeType(fileExtension.Value) : "application/octet-stream";

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };

            await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken);

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

    public Task<string> GetUrl(string fileName, Guid userId)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            
            if (blobClient.CanGenerateSasUri)
            {
                var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
                return Task.FromResult(sasUri.AbsoluteUri);
            }

            return Task.FromResult(blobClient.Uri.AbsoluteUri);
        }
        catch (Exception ex)
        {
            throw new StorageException(fileName, "Failed to generate URL for blob storage", ex);
        }
    }
}

