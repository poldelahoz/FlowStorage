using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using FlowStorage.Abstractions.Factories;
using FlowStorage.Abstractions.IBlobWrappers;
using FlowStorage.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowStorage.Services
{
    internal sealed class AzureBlobFlowStorage(
        string connectionString,
        IBlobServiceClientFactory blobServiceClientFactory,
        ILogger<IFlowStorage> logger)
        : IAzureBlobFlowStorage
    {
        private readonly IBlobServiceClientWrapper _blobServiceClient = blobServiceClientFactory.Create(connectionString);
        private readonly ILogger<IFlowStorage> _logger = logger;

        public async Task CopyFileAsync(string containerName, string sourceFilePath, string destFilePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(sourceFilePath, nameof(sourceFilePath));
            ArgumentException.ThrowIfNullOrEmpty(destFilePath, nameof(destFilePath));

            var containerClient = EnsureContainerExists(containerName);

            var sourceBlobClient = containerClient.GetBlobClient(sourceFilePath);
            var destinationBlobClient = containerClient.GetBlobClient(destFilePath);

            await destinationBlobClient.SyncCopyFromUriAsync(sourceBlobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(5)));
        }

        public async Task<bool> CreateContainerIfNotExistsAsync(string containerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(containerName, nameof(containerName));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            if (containerClient.Exists())
            {
                return false;
            }

            await containerClient.CreateIfNotExistsAsync();

            return true;
        }

        public async Task<bool> DeleteContainerIfExistsAsync(string containerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(containerName, nameof(containerName));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            if (!containerClient.Exists())
            {
                return false;
            }

            await containerClient.DeleteIfExistsAsync();

            return true;
        }

        public async Task DeleteFileIfExistsAsync(string containerName, string filePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            var containerClient = EnsureContainerExists(containerName);

            var blobClient = containerClient.GetBlobClient(filePath);

            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<Stream> DownloadFileAsync(string containerName, string filePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            var containerClient = EnsureContainerExists(containerName);

            var blobClient = containerClient.GetBlobClient(filePath);

            var response = await blobClient.DownloadAsync();

            return response.Value.Content;
        }

        public string GenerateSaSUri(string containerName, string filePath, DateTimeOffset expiryTime)
        {
            ArgumentException.ThrowIfNullOrEmpty(containerName, nameof(containerName));
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            var containerClient = EnsureContainerExists(containerName);

            var blobClient = containerClient.GetBlobClient(filePath);

            var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, expiryTime);

            return sasUri.ToString();
        }

        public async Task<string> ReadFileAsync(string containerName, string filePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            var containerClient = EnsureContainerExists(containerName);

            var blobClient = containerClient.GetBlobClient(filePath);

            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();

            return downloadResult.Content.ToString();
        }

        public async Task UploadFileAsync(string containerName, string filePath, string blobContents)
        {
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));
            ArgumentException.ThrowIfNullOrEmpty(blobContents, nameof(blobContents));

            var containerClient = EnsureContainerExists(containerName);

            var blobClient = containerClient.GetBlobClient(filePath);

            try
            {
                await blobClient.UploadAsync(BinaryData.FromString(JToken.Parse(blobContents).ToString(Formatting.Indented)), overwrite: true);
            }
            catch (Exception)
            {
                await blobClient.UploadAsync(BinaryData.FromString(blobContents), overwrite: true);
            }
        }

        public async Task UploadFileAsync(string containerName, string filePath, Stream fileStream)
        {
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            var containerClient = EnsureContainerExists(containerName);

            var blobClient = containerClient.GetBlobClient(filePath);

            await blobClient.UploadAsync(fileStream, overwrite: true);
        }

        private IBlobContainerClientWrapper EnsureContainerExists(string containerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(containerName, nameof(containerName));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            if (!containerClient.Exists())
            {
                throw new Exception("Container " + containerName + " does not exist.");
            }

            return containerClient;
        }
    }
}
