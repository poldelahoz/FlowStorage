using FlowStorage;
using FlowStorage.Services;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace FlowStorageTests.IntegrationTests.Tests
{
    public class LocalFlowStorageExtremeTests : IDisposable
    {
        private readonly string _basePath;
        private readonly LocalFlowStorage _localFlowStorage;

        public LocalFlowStorageExtremeTests()
        {
            _basePath = Path.Combine(Path.GetTempPath(), "LocalFlowStorage_ExtremeTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_basePath);

            var fileSystem = new FileSystem();
            var logger = new LoggerFactory().CreateLogger<IFlowStorage>();
            _localFlowStorage = new LocalFlowStorage(_basePath, fileSystem, logger);
        }

        [Fact]
        public async Task UploadDownloadFile_VeryLargeData_Succeeds()
        {
            // Arrange
            var containerName = "largeDataContainer";
            var fileName = "largeFile.txt";
            await _localFlowStorage.CreateContainerIfNotExistsAsync(containerName);

            int sizeInBytes = 1024 * 1024; // 1 MB
            var largeContent = new string('A', sizeInBytes);

            // Act
            await _localFlowStorage.UploadFileAsync(containerName, fileName, largeContent);

            // Act
            using var downloadStream = await _localFlowStorage.DownloadFileAsync(containerName, fileName);
            using var reader = new StreamReader(downloadStream);
            var downloadedContent = await reader.ReadToEndAsync();

            // Assert:
            Assert.Equal(largeContent, downloadedContent);
        }

        [Fact]
        public async Task UploadDownloadFile_SpecialCharacters_Succeeds()
        {
            // Arrange
            var containerName = "specialCharsContainer";
            var fileName = "tést_文件_ñ.txt";
            await _localFlowStorage.CreateContainerIfNotExistsAsync(containerName);

            var specialContent = "Contenido con caracteres especiales: á, é, í, ó, ú, ñ, ü, ¿, ¡, 😀, 🚀, ©, ™, λ, Ω";

            // Act
            await _localFlowStorage.UploadFileAsync(containerName, fileName, specialContent);

            // Act
            using var downloadStream = await _localFlowStorage.DownloadFileAsync(containerName, fileName);
            using var reader = new StreamReader(downloadStream);
            var downloadedContent = await reader.ReadToEndAsync();

            // Assert
            Assert.Equal(specialContent, downloadedContent);
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
