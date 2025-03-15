using FlowStorage.Factories;
using FlowStorage.Services;
using FlowStorage;
using FlowStorageTests.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Logging;

namespace FlowStorageTests.IntegrationTests.Tests
{
    [Collection("Azurite collection")]
    public class AzureBlobStorageTestsExtremeTests
    {
        private readonly AzuriteFixture _azuriteFixture;
        private readonly AzureBlobFlowStorage _azureBlobFlowStorage;
        private readonly string _containerName = "extremetestcontainer";

        public AzureBlobStorageTestsExtremeTests(AzuriteFixture fixture)
        {
            _azuriteFixture = fixture;

            var blobServiceClientFactory = new BlobServiceClientFactory();
            var logger = new LoggerFactory().CreateLogger<IFlowStorage>();

            _azureBlobFlowStorage = new AzureBlobFlowStorage(
                _azuriteFixture.ConnectionString,
                blobServiceClientFactory,
                logger);
        }

        [Fact]
        public async Task UploadDownloadFile_VeryLargeData_Succeeds()
        {
            // Arrange
            var filePath = "largeFile.txt";
            int sizeInBytes = 1024 * 1024;
            string largeContent = new('A', sizeInBytes);

            await _azureBlobFlowStorage.CreateContainerIfNotExistsAsync(_containerName);

            // Act
            await _azureBlobFlowStorage.UploadFileAsync(_containerName, filePath, largeContent);

            // Act
            string downloadedContent = await _azureBlobFlowStorage.ReadFileAsync(_containerName, filePath);

            // Assert
            Assert.Equal(largeContent, downloadedContent);
        }

        [Fact]
        public async Task UploadDownloadFile_SpecialCharacters_Succeeds()
        {
            // Arrange
            var filePath = "tést_文件_ñ.txt";
            var specialContent = "Contenido con caracteres especiales: á, é, í, ó, ú, ñ, ü, ¿, ¡, 😀, 🚀, ©, ™, λ, Ω";

            await _azureBlobFlowStorage.CreateContainerIfNotExistsAsync(_containerName);

            // Act
            await _azureBlobFlowStorage.UploadFileAsync(_containerName, filePath, specialContent);

            // Act
            string downloadedContent = await _azureBlobFlowStorage.ReadFileAsync(_containerName, filePath);

            // Assert
            Assert.Equal(specialContent, downloadedContent);
        }
    }
}
