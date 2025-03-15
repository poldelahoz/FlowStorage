using FlowStorage.Factories;
using FlowStorage.Services;
using FlowStorage;
using Microsoft.Extensions.Logging;
using FlowStorageTests.IntegrationTests.Infrastructure;
using Azure;
using Azure.Storage.Blobs;
using System.Text;

namespace FlowStorageTests.IntegrationTests.Tests
{
    [Collection("Azurite collection")]
    public class AzureBlobFlowStorageTests
    {
        private readonly AzuriteFixture _azuriteFixture;
        private readonly AzureBlobFlowStorage _azureBlobFlowStorage;
        private readonly string _containerName = "testcontainer";

        public AzureBlobFlowStorageTests(AzuriteFixture fixture)
        {
            _azuriteFixture = fixture;

            var blobServiceClientFactory = new BlobServiceClientFactory();
            var logger = new LoggerFactory().CreateLogger<IFlowStorage>();

            _azureBlobFlowStorage = new AzureBlobFlowStorage(
                _azuriteFixture.ConnectionString,
                blobServiceClientFactory,
                logger
            );
        }

        [Fact]
        public async Task CopyFileAsync_CopyFileSuccessfully()
        {
            // Arrange
            var sourcePath = "source.txt";
            var destPath = "dest.txt";
            var content = "this will be copied";
            await _azureBlobFlowStorage.CreateContainerIfNotExistsAsync(_containerName);
            await _azureBlobFlowStorage.UploadFileAsync(_containerName, sourcePath, content);

            // Act
            await _azureBlobFlowStorage.CopyFileAsync(_containerName, sourcePath, destPath);

            // Assert
            var downloadedContent = await _azureBlobFlowStorage.ReadFileAsync(_containerName, destPath);
            Assert.Equal(content, downloadedContent);
        }

        [Fact]
        public async Task CreateContainerIfNotExistsAsync_CreatesContainerIfNotExistsSuccessfully()
        {
            // Arrange
            await _azureBlobFlowStorage.DeleteContainerAsync(_containerName);

            // Act
            await _azureBlobFlowStorage.CreateContainerIfNotExistsAsync(_containerName);

            // Assert
            var blobServiceClient = new BlobServiceClient(_azuriteFixture.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var existsResponse = await containerClient.ExistsAsync();

            Assert.True(existsResponse.Value);
        }

        [Fact]
        public async Task DeleteContainerAsync_DeletesContainerIfExistsSuccessfully()
        {
            // Arrange
            await _azureBlobFlowStorage.CreateContainerIfNotExistsAsync(_containerName);

            // Act
            await _azureBlobFlowStorage.DeleteContainerAsync(_containerName);

            // Assert
            var blobServiceClient = new BlobServiceClient(_azuriteFixture.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var existsResponse = await containerClient.ExistsAsync();

            Assert.False(existsResponse.Value);
        }

        [Fact]
        public async Task ReadFileAsync_ReturnsFileContentSuccessfully()
        {
            // Arrange
            var filePath = "test.txt";
            var content = "Hello, FlowStorage!";
            await _azureBlobFlowStorage.CreateContainerIfNotExistsAsync(_containerName);
            await _azureBlobFlowStorage.UploadFileAsync(_containerName, filePath, content);

            // Act
            var downloadedContent = await _azureBlobFlowStorage.ReadFileAsync(_containerName, filePath);

            // Assert
            Assert.Equal(content, downloadedContent);
        }

        [Fact]
        public async Task UploadFileAsync_WithString_UploadsFileSuccessfully()
        {
            // Arrange
            var filePath = "test.txt";
            var content = "Hello, FlowStorage!";
            await _azureBlobFlowStorage.CreateContainerIfNotExistsAsync(_containerName);

            // Act
            await _azureBlobFlowStorage.UploadFileAsync(_containerName, filePath, content);

            // Assert
            var downloadedContent = await _azureBlobFlowStorage.ReadFileAsync(_containerName, filePath);
            Assert.Equal(content, downloadedContent);
        }

        [Fact]
        public async Task UploadFileAsync_WithStream_UploadsFileSuccessfully()
        {
            // Arrange
            var filePath = "test.txt";
            var expectedContent = "Hello, FlowStorage!";
            await _azureBlobFlowStorage.CreateContainerIfNotExistsAsync(_containerName);

            var contentBytes = Encoding.UTF8.GetBytes(expectedContent);
            using var uploadStream = new MemoryStream(contentBytes);

            // Act
            await _azureBlobFlowStorage.UploadFileAsync(_containerName, filePath, uploadStream);

            // Assert
            var downloadedContent = await _azureBlobFlowStorage.ReadFileAsync(_containerName, filePath);
            Assert.Equal(expectedContent, downloadedContent);
        }

        [Fact]
        public async Task DownloadFileAsync_ReturnsCorrectContent()
        {
            // Arrange
            var filePath = "file.txt";
            var originalContent = "Contenido de prueba";
            await _azureBlobFlowStorage.CreateContainerIfNotExistsAsync(_containerName);
            await _azureBlobFlowStorage.UploadFileAsync(_containerName, filePath, originalContent);

            // Act
            using var stream = await _azureBlobFlowStorage.DownloadFileAsync(_containerName, filePath);
            using var reader = new StreamReader(stream);
            var actualContent = await reader.ReadToEndAsync();

            // Assert
            Assert.Equal(originalContent, actualContent);
        }

        [Fact]
        public async Task DeleteFileAsync_RemovesFileFromBlobStorage()
        {
            // Arrange
            var filePath = "delete.txt";
            var content = "This will be deleted.";
            await _azureBlobFlowStorage.CreateContainerIfNotExistsAsync(_containerName);
            await _azureBlobFlowStorage.UploadFileAsync(_containerName, filePath, content);

            // Act
            await _azureBlobFlowStorage.DeleteFileAsync(_containerName, filePath);

            // Assert
            await Assert.ThrowsAsync<RequestFailedException>(() => _azureBlobFlowStorage.ReadFileAsync(_containerName, filePath));
        }
    }
}
