using FlowStorage.Services;
using FlowStorage;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Text;

namespace FlowStorageTests.IntegrationTests.Tests
{
    public class LocalFlowStorageTests : IDisposable
    {
        private readonly string _basePath;
        private readonly LocalFlowStorage _localFlowStorage;
        private readonly string _containerName = "testcontainer";

        public LocalFlowStorageTests()
        {
            _basePath = Path.Combine(Path.GetTempPath(), "LocalFlowStorageTests");
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            var fileSystem = new FileSystem();
            var loggerMock = new LoggerFactory().CreateLogger<IFlowStorage>();

            _localFlowStorage = new LocalFlowStorage(_basePath, fileSystem, loggerMock);
        }

        [Fact]
        public async Task CopyFileAsync_CopyFileSuccessfully()
        {
            // Arrange
            var sourcePath = "source.txt";
            var destPath = "dest.txt";
            var expectedContent = "this will be copied";
            await _localFlowStorage.CreateContainerIfNotExistsAsync(_containerName);
            await _localFlowStorage.UploadFileAsync(_containerName, sourcePath, expectedContent);

            // Act
            await _localFlowStorage.CopyFileAsync(_containerName, sourcePath, destPath);

            // Assert
            var filePath = Path.Combine(_basePath, _containerName, destPath);
            Assert.True(File.Exists(filePath));
            Assert.Equal(expectedContent, await File.ReadAllTextAsync(filePath));
        }

        [Fact]
        public async Task CreateContainerIfNotExistsAsync_CreatesContainerIfNotExistsSuccessfully_And_ReturnsTrue()
        {
            // Arrange
            string expectedPath = Path.Combine(_basePath, _containerName);

            if (Directory.Exists(expectedPath))
            {
                Directory.Delete(expectedPath, recursive: true);
            }

            // Act
            var result = await _localFlowStorage.CreateContainerIfNotExistsAsync(_containerName);

            // Assert
            Assert.True(result);
            Assert.True(Directory.Exists(expectedPath));
        }

        [Fact]
        public async Task CreateContainerIfNotExistsAsync_ReturnsFalse_IfContainerAlreadyExists()
        {
            // Arrange
            string expectedPath = Path.Combine(_basePath, _containerName);
            Directory.CreateDirectory(expectedPath);

            // Act
            var result = await _localFlowStorage.CreateContainerIfNotExistsAsync(_containerName);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteContainerAsync_DeletesContainerIfExistsSuccessfully_AndReturnsTrue()
        {
            // Arrange
            string expectedPath = Path.Combine(_basePath, _containerName);
            await _localFlowStorage.CreateContainerIfNotExistsAsync(_containerName);

            // Act
            var result = await _localFlowStorage.DeleteContainerIfExistsAsync(_containerName);

            // Assert
            Assert.True(result);
            Assert.False(Directory.Exists(expectedPath));
        }

        [Fact]
        public async Task DeleteContainerAsync_ReturnsFalse_IfContainerNotExists()
        {
            // Arrange
            string expectedPath = Path.Combine(_basePath, _containerName);

            if (Directory.Exists(expectedPath))
            {
                Directory.Delete(expectedPath, recursive: true);
            }

            // Act
            var result = await _localFlowStorage.DeleteContainerIfExistsAsync(_containerName);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ReadFileAsync_ReturnsFileContentSuccessfully()
        {
            // Arrange
            var filePath = "test.txt";
            var content = "Hello, FlowStorage!";
            await _localFlowStorage.CreateContainerIfNotExistsAsync(_containerName);
            await _localFlowStorage.UploadFileAsync(_containerName, filePath, content);

            // Act
            var downloadedContent = await _localFlowStorage.ReadFileAsync(_containerName, filePath);

            // Assert
            Assert.Equal(content, downloadedContent);
        }

        [Fact]
        public async Task UploadFileAsync__WithString_CreatesFile_WithCorrectContent()
        {
            // Arrange
            var fileName = "test.txt";
            var content = "Hello, Local Storage!";
            await _localFlowStorage.CreateContainerIfNotExistsAsync(_containerName);

            // Act
            await _localFlowStorage.UploadFileAsync(_containerName, fileName, content);

            // Assert
            var filePath = Path.Combine(_basePath, _containerName, fileName);
            Assert.True(File.Exists(filePath));
            Assert.Equal(content, await File.ReadAllTextAsync(filePath));
        }

        [Fact]
        public async Task UploadFileAsync__WithSteam_CreatesFile_WithCorrectContent()
        {
            // Arrange
            var fileName = "test.txt";
            var expectedContent = "Hello, Local Storage!";
            await _localFlowStorage.CreateContainerIfNotExistsAsync(_containerName);

            var contentBytes = Encoding.UTF8.GetBytes(expectedContent);
            using var uploadStream = new MemoryStream(contentBytes);

            // Act
            await _localFlowStorage.UploadFileAsync(_containerName, fileName, expectedContent);

            // Assert
            var filePath = Path.Combine(_basePath, _containerName, fileName);
            Assert.True(File.Exists(filePath));
            Assert.Equal(expectedContent, await File.ReadAllTextAsync(filePath));
        }

        [Fact]
        public async Task DownloadFileAsync_ReturnsCorrectContent()
        {
            // Arrange
            var fileName = "test.txt";
            var expectedContent = "This is a test file.";
            await _localFlowStorage.CreateContainerIfNotExistsAsync(_containerName);
            var filePath = Path.Combine(_basePath, _containerName, fileName);
            await File.WriteAllTextAsync(filePath, expectedContent);

            // Act
            using var stream = await _localFlowStorage.DownloadFileAsync(_containerName, fileName);
            using var reader = new StreamReader(stream);
            var actualContent = await reader.ReadToEndAsync();

            // Assert
            Assert.Equal(expectedContent, actualContent);
        }

        [Fact]
        public async Task DeleteFileIfExistsAsync_RemovesFileFromBlobStorage()
        {
            // Arrange
            var filePath = "delete.txt";
            var content = "This will be deleted.";
            await _localFlowStorage.CreateContainerIfNotExistsAsync(_containerName);
            await _localFlowStorage.UploadFileAsync(_containerName, filePath, content);

            // Act
            await _localFlowStorage.DeleteFileIfExistsAsync(_containerName, filePath);

            // Assert
            await Assert.ThrowsAsync<Exception>(() => _localFlowStorage.ReadFileAsync(_containerName, filePath));
        }

        public void Dispose()
        {
            if (Directory.Exists(_basePath))
            {
                Directory.Delete(_basePath, recursive: true);
            }
        }
    }
}
