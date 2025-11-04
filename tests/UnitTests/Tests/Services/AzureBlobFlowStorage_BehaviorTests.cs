using FlowStorage.Abstractions.IBlobWrappers;
using FlowStorage.Services;
using FlowStorage;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Azure.Storage.Sas;
using FlowStorageTests.UnitTests.Helpers;
using FlowStorage.Abstractions.Factories;
using FlowStorage.Abstractions.Services;

namespace FlowStorageTests.UnitTests.Tests.Services
{
    public class AzureBlobFlowStorage_BehaviorTests
    {
        [Fact]
        public async Task CopyFileAsync_CallsSyncCopyFromUriAsync()
        {
            // Arrange
            var containerName = "testContainer";
            var sourceFile = "source.txt";
            var destFile = "dest.txt";
            var fakeSasUri = new Uri("https://fake.blob.core.windows.net/testContainer/source.txt?sasToken");

            var mocks = SetupCopyFileMocks(containerName, sourceFile, destFile, fakeSasUri);

            var blobServiceClientFactoryMock = new Mock<IBlobServiceClientFactory>();
            blobServiceClientFactoryMock.Setup(x => x.Create(It.IsAny<string>()))
                                        .Returns(mocks.serviceClientWrapperMock.Object);

            var loggerMock = new Mock<ILogger<IFlowStorage>>();
            var azureBlobFlowStorage = new AzureBlobFlowStorage("dummyConnectionString", blobServiceClientFactoryMock.Object, loggerMock.Object);

            // Act
            await azureBlobFlowStorage.CopyFileAsync(containerName, sourceFile, destFile);

            // Assert: Se debe haber llamado SyncCopyFromUriAsync en el blob destino con el SAS URI generado
            mocks.destBlobClientMock.Verify(x => x.SyncCopyFromUriAsync(fakeSasUri), Times.Once);
        }

        [Fact]
        public async Task DeleteContainerAsync_CallsDeleteIfExistsAsync()
        {
            // Arrange
            var containerName = "testContainer";

            var containerClientMock = new Mock<IBlobContainerClientWrapper>();
            // Simulamos que el contenedor existe
            containerClientMock.Setup(x => x.Exists()).Returns(true);
            containerClientMock.Setup(x => x.DeleteIfExistsAsync()).Returns(Task.CompletedTask).Verifiable();

            var azureBlobFlowStorage = CreateAzureBlobFlowStorageWithContainerMock(containerName, containerClientMock.Object);

            // Act
            await azureBlobFlowStorage.DeleteContainerIfExistsAsync(containerName);

            // Assert
            containerClientMock.Verify(x => x.DeleteIfExistsAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteFileIfExistsAsync_CallsDeleteIfExistsAsync_OnBlobClient()
        {
            // Arrange
            var containerName = "testContainer";
            var filePath = "file.txt";

            var blobClientMock = new Mock<IBlobClientWrapper>();
            blobClientMock.Setup(x => x.DeleteIfExistsAsync())
                          .Returns(Task.CompletedTask)
                          .Verifiable();

            var containerMock = new Mock<IBlobContainerClientWrapper>();
            containerMock.Setup(x => x.Exists()).Returns(true);
            containerMock.Setup(x => x.GetBlobClient(filePath))
                         .Returns(blobClientMock.Object);

            var azureStorage = CreateAzureBlobFlowStorageWithContainerMock(containerName, containerMock.Object);


            // Act
            await azureStorage.DeleteFileIfExistsAsync(containerName, filePath);

            // Assert
            blobClientMock.Verify(x => x.DeleteIfExistsAsync(), Times.Once);
        }

        [Fact]
        public async Task ReadFileAsync_ReturnsContent_AsString_WhenFileExists()
        {
            // Arrange
            var containerName = "testContainer";
            var filePath = "file.txt";
            var expectedContent = "File content as string";

            var fakeDownloadResult = BlobDownloadResultFactory.CreateFake(expectedContent);

            var (azureStorage, _, blobClientMock) = SetupAzureStorageForBlobOperation(containerName, filePath);

            blobClientMock.Setup(x => x.DownloadContentAsync())
                          .ReturnsAsync(fakeDownloadResult);

            // Act
            string content = await azureStorage.ReadFileAsync(containerName, filePath);

            // Assert
            Assert.Equal(expectedContent, content);
        }

        [Fact]
        public async Task UploadFileAsync_WithString_CallsUploadAsync_TwiceIfFirstFails()
        {
            // Arrange
            var containerName = "testContainer";
            var filePath = "file.txt";
            var blobContents = "{\"key\":\"value\"}"; // Contenido JSON válido

            var (azureStorage, _, blobClientMock) = SetupAzureStorageForBlobOperation(containerName, filePath);

            // Configuramos la secuencia: la primera llamada falla, la segunda tiene éxito.
            blobClientMock
                .SetupSequence(x => x.UploadAsync(It.IsAny<BinaryData>(), true, null))
                .ThrowsAsync(new Exception("Simulated failure"))
                .Returns(Task.CompletedTask);

            // Act
            await azureStorage.UploadFileAsync(containerName, filePath, blobContents);

            // Assert: Verificamos que UploadAsync se haya llamado dos veces.
            blobClientMock.Verify(x => x.UploadAsync(It.IsAny<BinaryData>(), true, null), Times.Exactly(2));
        }

        [Fact]
        public async Task UploadFileAsync_WithStream_CallsUploadAsync()
        {
            // Arrange
            var containerName = "testContainer";
            var filePath = "file.txt";
            var contentToUpload = "Stream upload content";
            var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(contentToUpload));

            var (azureStorage, _, blobClientMock) = SetupAzureStorageForBlobOperation(containerName, filePath);

            // Configuramos el mock para que al llamar a UploadAsync con stream retorne una tarea completada.
            blobClientMock.Setup(x => x.UploadAsync(It.IsAny<Stream>(), true, null))
                          .Returns(Task.CompletedTask)
                          .Verifiable();

            // Act
            await azureStorage.UploadFileAsync(containerName, filePath, uploadStream);

            // Assert
            blobClientMock.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true, null), Times.Once);
        }

        [Fact]
        public void GenerateSaSUri_CallsGenerateSasUri_OnBlobClient()
        {
            // Arrange
            var containerName = "testContainer";
            var filePath = "file.txt";
            var expiryTime = DateTimeOffset.UtcNow.AddHours(1);
            var expectedSasUri = new Uri("https://fake.blob.core.windows.net/testContainer/file.txt?sasToken");

            var (azureStorage, _, blobClientMock) = SetupAzureStorageForBlobOperation(containerName, filePath);

            blobClientMock.Setup(x => x.GenerateSasUri(BlobSasPermissions.Read, expiryTime))
                          .Returns(expectedSasUri)
                          .Verifiable();

            // Act
            string sasUri = azureStorage.GenerateSaSUri(containerName, filePath, expiryTime);

            // Assert
            Assert.Equal(expectedSasUri.ToString(), sasUri);
            blobClientMock.Verify(x => x.GenerateSasUri(BlobSasPermissions.Read, expiryTime), Times.Once);
        }

        private (Mock<IBlobClientWrapper> sourceBlobClientMock, Mock<IBlobClientWrapper> destBlobClientMock,
         Mock<IBlobContainerClientWrapper> containerClientMock, Mock<IBlobServiceClientWrapper> serviceClientWrapperMock)
            SetupCopyFileMocks(string containerName, string sourceFile, string destFile, Uri fakeSasUri)
        {
            var sourceBlobClientMock = new Mock<IBlobClientWrapper>();
            sourceBlobClientMock.Setup(x => x.GenerateSasUri(It.IsAny<BlobSasPermissions>(), It.IsAny<DateTimeOffset>()))
                                .Returns(fakeSasUri);

            var destBlobClientMock = new Mock<IBlobClientWrapper>();

            var containerClientMock = new Mock<IBlobContainerClientWrapper>();
            containerClientMock.Setup(x => x.Exists()).Returns(true);
            containerClientMock.Setup(x => x.GetBlobClient(sourceFile))
                               .Returns(sourceBlobClientMock.Object);
            containerClientMock.Setup(x => x.GetBlobClient(destFile))
                               .Returns(destBlobClientMock.Object);

            var serviceClientWrapperMock = new Mock<IBlobServiceClientWrapper>();
            serviceClientWrapperMock.Setup(x => x.GetBlobContainerClient(containerName))
                                    .Returns(containerClientMock.Object);

            return (sourceBlobClientMock, destBlobClientMock, containerClientMock, serviceClientWrapperMock);
        }

        private IAzureBlobFlowStorage CreateAzureBlobFlowStorageWithContainerMock(
            string containerName,
            IBlobContainerClientWrapper containerMock)
        {
            var serviceClientWrapperMock = new Mock<IBlobServiceClientWrapper>();
            serviceClientWrapperMock.Setup(x => x.GetBlobContainerClient(containerName))
                                    .Returns(containerMock);

            var blobServiceClientFactoryMock = new Mock<IBlobServiceClientFactory>();
            blobServiceClientFactoryMock.Setup(x => x.Create(It.IsAny<string>()))
                                        .Returns(serviceClientWrapperMock.Object);

            var loggerMock = new Mock<ILogger<IFlowStorage>>();
            return new AzureBlobFlowStorage("dummyConnectionString", blobServiceClientFactoryMock.Object, loggerMock.Object);
        }

        private (AzureBlobFlowStorage azureStorage, Mock<IBlobContainerClientWrapper> containerMock, Mock<IBlobClientWrapper> blobClientMock)
            SetupAzureStorageForBlobOperation(string containerName, string filePath)
        {
            var blobClientMock = new Mock<IBlobClientWrapper>();

            var containerMock = new Mock<IBlobContainerClientWrapper>();
            containerMock.Setup(x => x.Exists()).Returns(true);
            containerMock.Setup(x => x.GetBlobClient(filePath))
                         .Returns(blobClientMock.Object);

            var serviceClientWrapperMock = new Mock<IBlobServiceClientWrapper>();
            serviceClientWrapperMock.Setup(x => x.GetBlobContainerClient(containerName))
                                    .Returns(containerMock.Object);

            var factoryMock = new Mock<IBlobServiceClientFactory>();
            factoryMock.Setup(x => x.Create(It.IsAny<string>()))
                       .Returns(serviceClientWrapperMock.Object);

            var loggerMock = new Mock<ILogger<IFlowStorage>>();
            var azureStorage = new AzureBlobFlowStorage("dummyConnectionString", factoryMock.Object, loggerMock.Object);

            return (azureStorage, containerMock, blobClientMock);
        }
    }
}
