using FlowStorage.Services;
using FlowStorage;
using Microsoft.Extensions.Logging;
using Moq;
using FlowStorage.Abstractions.Factories;

namespace FlowStorageTests.UnitTests.Tests.Services
{
    public class AzureBlobFlowStorage_ParameterTests
    {
        private static AzureBlobFlowStorage CreateAzureBlobFlowStorage(
            IBlobServiceClientFactory? blobServiceClientFactory = null,
            ILogger<IFlowStorage>? logger = null)
        {
            blobServiceClientFactory ??= new Mock<IBlobServiceClientFactory>().Object;
            logger ??= new Mock<ILogger<IFlowStorage>>().Object;
            return new AzureBlobFlowStorage("dummyConnectionString", blobServiceClientFactory, logger);
        }

        [Fact]
        public async Task CopyFileAsync_ThrowsArgumentException_WhenContainerNameIsEmpty()
        {
            // Arrange
            var azureStorage = CreateAzureBlobFlowStorage();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.CopyFileAsync(string.Empty, "source.txt", "dest.txt"));
        }

        [Fact]
        public async Task CopyFileAsync_ThrowsArgumentNullException_WhenContainerNameIsNull()
        {
            // Arrange
            var azureStorage = CreateAzureBlobFlowStorage();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.CopyFileAsync(null!, "source.txt", "dest.txt"));
        }

        [Fact]
        public async Task CopyFileAsync_ThrowsArgumentException_WhenSourceFilePathIsEmpty()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.CopyFileAsync("container", string.Empty, "dest.txt"));
        }

        [Fact]
        public async Task CopyFileAsync_ThrowsArgumentNullException_WhenSourceFilePathIsNull()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.CopyFileAsync("container", null!, "dest.txt"));
        }

        [Fact]
        public async Task CopyFileAsync_ThrowsArgumentException_WhenDestFilePathIsEmpty()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.CopyFileAsync("container", "source.txt", string.Empty));
        }

        [Fact]
        public async Task CopyFileAsync_ThrowsArgumentNullException_WhenDestFilePathIsNull()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.CopyFileAsync("container", "source.txt", null!));
        }

        [Fact]
        public async Task CreateContainerIfNotExistsAsync_ThrowsArgumentException_WhenContainerNameIsEmpty()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.CreateContainerIfNotExistsAsync(string.Empty));
        }

        [Fact]
        public async Task CreateContainerIfNotExistsAsync_ThrowsArgumentNullException_WhenContainerNameIsNull()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.CreateContainerIfNotExistsAsync(null!));
        }

        [Fact]
        public async Task DeleteContainerAsync_ThrowsArgumentException_WhenContainerNameIsEmpty()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.DeleteContainerIfExistsAsync(string.Empty));
        }

        [Fact]
        public async Task DeleteContainerAsync_ThrowsArgumentNullException_WhenContainerNameIsNull()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.DeleteContainerIfExistsAsync(null!));
        }

        [Fact]
        public async Task DeleteFileIfExistsAsync_ThrowsArgumentException_WhenContainerNameIsEmpty()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.DeleteFileIfExistsAsync(string.Empty, "file.txt"));
        }

        [Fact]
        public async Task DeleteFileAsync_ThrowsArgumentNullException_WhenContainerNameIsNull()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.DeleteFileIfExistsAsync(null!, "file.txt"));
        }

        [Fact]
        public async Task DeleteFileIfExistsAsync_ThrowsArgumentException_WhenFilePathIsEmpty()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.DeleteFileIfExistsAsync("container", string.Empty));
        }

        [Fact]
        public async Task DeleteFileIfExistsAsync_ThrowsArgumentNullException_WhenFilePathIsNull()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.DeleteFileIfExistsAsync("container", null!));
        }

        [Fact]
        public async Task DownloadFileAsync_ThrowsArgumentException_WhenContainerNameIsEmpty()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.DownloadFileAsync(string.Empty, "file.txt"));
        }

        [Fact]
        public async Task DownloadFileAsync_ThrowsArgumentNullException_WhenContainerNameIsNull()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.DownloadFileAsync(null!, "file.txt"));
        }

        [Fact]
        public async Task DownloadFileAsync_ThrowsArgumentException_WhenFilePathIsEmpty()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.DownloadFileAsync("container", string.Empty));
        }

        [Fact]
        public async Task DownloadFileAsync_ThrowsArgumentNullException_WhenFilePathIsNull()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.DownloadFileAsync("container", null!));
        }

        [Fact]
        public async Task ReadFileAsync_ThrowsArgumentException_WhenContainerNameIsEmpty()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.ReadFileAsync(string.Empty, "file.txt"));
        }

        [Fact]
        public async Task ReadFileAsync_ThrowsArgumentNullException_WhenContainerNameIsNull()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.ReadFileAsync(null!, "file.txt"));
        }

        [Fact]
        public async Task ReadFileAsync_ThrowsArgumentException_WhenFilePathIsEmpty()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.ReadFileAsync("container", string.Empty));
        }

        [Fact]
        public async Task ReadFileAsync_ThrowsArgumentNullException_WhenFilePathIsNull()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.ReadFileAsync("container", null!));
        }

        [Fact]
        public async Task UploadFileAsync_WithString_ThrowsArgumentException_WhenParametersAreEmpty()
        {
            var azureStorage = CreateAzureBlobFlowStorage();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.UploadFileAsync(string.Empty, "file.txt", "content"));
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.UploadFileAsync("container", string.Empty, "content"));
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.UploadFileAsync("container", "file.txt", string.Empty));
        }

        [Fact]
        public async Task UploadFileAsync_WithString_ThrowsArgumentNullException_WhenParametersAreNull()
        {
            var azureStorage = CreateAzureBlobFlowStorage();

            string nullString = null!;
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.UploadFileAsync(null!, "file.txt", "content"));
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.UploadFileAsync("container", null!, "content"));
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.UploadFileAsync("container", "file.txt", nullString));
        }

        [Fact]
        public async Task UploadFileAsync_WithStream_ThrowsArgumentException_WhenParametersAreEmpty()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.UploadFileAsync(string.Empty, "file.txt", new MemoryStream()));
            await Assert.ThrowsAsync<ArgumentException>(() =>
                azureStorage.UploadFileAsync("container", string.Empty, new MemoryStream()));
        }

        [Fact]
        public async Task UploadFileAsync_WithStream_ThrowsArgumentNullException_WhenParametersAreNull()
        {
            var azureStorage = CreateAzureBlobFlowStorage();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.UploadFileAsync(null!, "file.txt", new MemoryStream()));
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                azureStorage.UploadFileAsync("container", null!, new MemoryStream()));
        }
    }
}