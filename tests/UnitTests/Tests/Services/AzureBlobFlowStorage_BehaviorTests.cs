using FlowStorage.Abstractions.IBlobWrappers;
using FlowStorage.Abstractions;
using FlowStorage.Services;
using FlowStorage;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azure.Storage.Sas;
using FlowStorageTests.UnitTests.Helpers;

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

            // Mocks para los blob clients
            var sourceBlobClientMock = new Mock<IBlobClientWrapper>();
            var destBlobClientMock = new Mock<IBlobClientWrapper>();

            // Configuramos el mock para que, al generar el SAS, retorne una URI fija
            var fakeSasUri = new Uri("https://fake.blob.core.windows.net/testContainer/source.txt?sasToken");
            sourceBlobClientMock
                .Setup(x => x.GenerateSasUri(It.IsAny<BlobSasPermissions>(), It.IsAny<DateTimeOffset>()))
                .Returns(fakeSasUri);

            // Configuramos el container wrapper para que devuelva los blob clients simulados
            var containerClientMock = new Mock<IBlobContainerClientWrapper>();
            containerClientMock.Setup(x => x.Exists()).Returns(true);
            containerClientMock.Setup(x => x.GetBlobClient(sourceFile)).Returns(sourceBlobClientMock.Object);
            containerClientMock.Setup(x => x.GetBlobClient(destFile)).Returns(destBlobClientMock.Object);

            // Configuramos el blob service client wrapper para que devuelva el container mock
            var blobServiceClientWrapperMock = new Mock<IBlobServiceClientWrapper>();
            blobServiceClientWrapperMock.Setup(x => x.GetBlobContainerClient(containerName))
                                        .Returns(containerClientMock.Object);

            // Configuramos el factory para que retorne el blob service client wrapper simulado
            var blobServiceClientFactoryMock = new Mock<IBlobServiceClientFactory>();
            blobServiceClientFactoryMock.Setup(x => x.Create(It.IsAny<string>()))
                                        .Returns(blobServiceClientWrapperMock.Object);

            var loggerMock = new Mock<ILogger<IFlowStorage>>();

            var azureBlobFlowStorage = new AzureBlobFlowStorage("dummyConnectionString", blobServiceClientFactoryMock.Object, loggerMock.Object);

            // Act
            await azureBlobFlowStorage.CopyFileAsync(containerName, sourceFile, destFile);

            // Assert: Se debe haber llamado SyncCopyFromUriAsync en el blob destino con el SAS URI generado
            destBlobClientMock.Verify(x => x.SyncCopyFromUriAsync(fakeSasUri), Times.Once);
        }

        [Fact]
        public async Task CreateContainerIfNotExistsAsync_CallsCreateIfNotExistsAsync()
        {
            // Arrange
            var containerName = "testContainer";

            var containerClientMock = new Mock<IBlobContainerClientWrapper>();
            // Simulamos que el contenedor no existe para que se invoque CreateIfNotExistsAsync
            containerClientMock.Setup(x => x.Exists()).Returns(false);
            containerClientMock.Setup(x => x.CreateIfNotExistsAsync()).Returns(Task.CompletedTask).Verifiable();

            var blobServiceClientWrapperMock = new Mock<IBlobServiceClientWrapper>();
            blobServiceClientWrapperMock.Setup(x => x.GetBlobContainerClient(containerName))
                                        .Returns(containerClientMock.Object);

            var blobServiceClientFactoryMock = new Mock<IBlobServiceClientFactory>();
            blobServiceClientFactoryMock.Setup(x => x.Create(It.IsAny<string>()))
                                        .Returns(blobServiceClientWrapperMock.Object);

            var loggerMock = new Mock<ILogger<IFlowStorage>>();
            var azureBlobFlowStorage = new AzureBlobFlowStorage("dummyConnectionString", blobServiceClientFactoryMock.Object, loggerMock.Object);

            // Act
            await azureBlobFlowStorage.CreateContainerIfNotExistsAsync(containerName);

            // Assert
            containerClientMock.Verify(x => x.CreateIfNotExistsAsync(), Times.Once);
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

            var blobServiceClientWrapperMock = new Mock<IBlobServiceClientWrapper>();
            blobServiceClientWrapperMock.Setup(x => x.GetBlobContainerClient(containerName))
                                        .Returns(containerClientMock.Object);

            var blobServiceClientFactoryMock = new Mock<IBlobServiceClientFactory>();
            blobServiceClientFactoryMock.Setup(x => x.Create(It.IsAny<string>()))
                                        .Returns(blobServiceClientWrapperMock.Object);

            var loggerMock = new Mock<ILogger<IFlowStorage>>();
            var azureBlobFlowStorage = new AzureBlobFlowStorage("dummyConnectionString", blobServiceClientFactoryMock.Object, loggerMock.Object);

            // Act
            await azureBlobFlowStorage.DeleteContainerAsync(containerName);

            // Assert
            containerClientMock.Verify(x => x.DeleteIfExistsAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteFileAsync_CallsDeleteIfExistsAsync_OnBlobClient()
        {
            // Arrange
            var containerName = "testContainer";
            var filePath = "file.txt";

            var blobClientMock = new Mock<IBlobClientWrapper>();
            blobClientMock.Setup(x => x.DeleteIfExistsAsync()).Returns(Task.CompletedTask).Verifiable();

            var containerClientMock = new Mock<IBlobContainerClientWrapper>();
            containerClientMock.Setup(x => x.Exists()).Returns(true);
            containerClientMock.Setup(x => x.GetBlobClient(filePath)).Returns(blobClientMock.Object);

            var blobServiceClientWrapperMock = new Mock<IBlobServiceClientWrapper>();
            blobServiceClientWrapperMock.Setup(x => x.GetBlobContainerClient(containerName))
                                        .Returns(containerClientMock.Object);

            var blobServiceClientFactoryMock = new Mock<IBlobServiceClientFactory>();
            blobServiceClientFactoryMock.Setup(x => x.Create(It.IsAny<string>()))
                                        .Returns(blobServiceClientWrapperMock.Object);

            var loggerMock = new Mock<ILogger<IFlowStorage>>();
            var azureBlobFlowStorage = new AzureBlobFlowStorage("dummyConnectionString", blobServiceClientFactoryMock.Object, loggerMock.Object);

            // Act
            await azureBlobFlowStorage.DeleteFileAsync(containerName, filePath);

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

            var blobClientMock = new Mock<IBlobClientWrapper>();
            blobClientMock.Setup(x => x.DownloadContentAsync())
                          .ReturnsAsync(fakeDownloadResult);

            var containerClientMock = new Mock<IBlobContainerClientWrapper>();
            containerClientMock.Setup(x => x.Exists()).Returns(true);
            containerClientMock.Setup(x => x.GetBlobClient(filePath)).Returns(blobClientMock.Object);

            var blobServiceClientWrapperMock = new Mock<IBlobServiceClientWrapper>();
            blobServiceClientWrapperMock.Setup(x => x.GetBlobContainerClient(containerName))
                                        .Returns(containerClientMock.Object);

            var blobServiceClientFactoryMock = new Mock<IBlobServiceClientFactory>();
            blobServiceClientFactoryMock.Setup(x => x.Create(It.IsAny<string>()))
                                        .Returns(blobServiceClientWrapperMock.Object);

            var loggerMock = new Mock<ILogger<IFlowStorage>>();
            var azureStorage = new AzureBlobFlowStorage("dummyConnectionString", blobServiceClientFactoryMock.Object, loggerMock.Object);

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

            var blobClientMock = new Mock<IBlobClientWrapper>();
            // Simulamos que la primera llamada falla (por ejemplo, por no poder formatear el JSON)
            blobClientMock
                .SetupSequence(x => x.UploadAsync(It.IsAny<BinaryData>(), true))
                .ThrowsAsync(new Exception("Simulated failure"))
                .Returns(Task.CompletedTask);

            var containerClientMock = new Mock<IBlobContainerClientWrapper>();
            containerClientMock.Setup(x => x.Exists()).Returns(true);
            containerClientMock.Setup(x => x.GetBlobClient(filePath)).Returns(blobClientMock.Object);

            var blobServiceClientWrapperMock = new Mock<IBlobServiceClientWrapper>();
            blobServiceClientWrapperMock.Setup(x => x.GetBlobContainerClient(containerName))
                                        .Returns(containerClientMock.Object);

            var blobServiceClientFactoryMock = new Mock<IBlobServiceClientFactory>();
            blobServiceClientFactoryMock.Setup(x => x.Create(It.IsAny<string>()))
                                        .Returns(blobServiceClientWrapperMock.Object);

            var loggerMock = new Mock<ILogger<IFlowStorage>>();
            var azureStorage = new AzureBlobFlowStorage("dummyConnectionString", blobServiceClientFactoryMock.Object, loggerMock.Object);

            // Act
            await azureStorage.UploadFileAsync(containerName, filePath, blobContents);

            // Assert: Verificamos que UploadAsync se haya llamado dos veces.
            blobClientMock.Verify(x => x.UploadAsync(It.IsAny<BinaryData>(), true), Times.Exactly(2));
        }

        [Fact]
        public async Task UploadFileAsync_WithStream_CallsUploadAsync()
        {
            // Arrange
            var containerName = "testContainer";
            var filePath = "file.txt";
            var contentToUpload = "Stream upload content";
            var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(contentToUpload));

            var blobClientMock = new Mock<IBlobClientWrapper>();
            blobClientMock.Setup(x => x.UploadAsync(It.IsAny<Stream>(), true))
                          .Returns(Task.CompletedTask)
                          .Verifiable();

            var containerClientMock = new Mock<IBlobContainerClientWrapper>();
            containerClientMock.Setup(x => x.Exists()).Returns(true);
            containerClientMock.Setup(x => x.GetBlobClient(filePath)).Returns(blobClientMock.Object);

            var blobServiceClientWrapperMock = new Mock<IBlobServiceClientWrapper>();
            blobServiceClientWrapperMock.Setup(x => x.GetBlobContainerClient(containerName))
                                        .Returns(containerClientMock.Object);

            var blobServiceClientFactoryMock = new Mock<IBlobServiceClientFactory>();
            blobServiceClientFactoryMock.Setup(x => x.Create(It.IsAny<string>()))
                                        .Returns(blobServiceClientWrapperMock.Object);

            var loggerMock = new Mock<ILogger<IFlowStorage>>();
            var azureStorage = new AzureBlobFlowStorage("dummyConnectionString", blobServiceClientFactoryMock.Object, loggerMock.Object);

            // Act
            await azureStorage.UploadFileAsync(containerName, filePath, uploadStream);

            // Assert
            blobClientMock.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true), Times.Once);
        }
    }
}
