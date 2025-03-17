using FlowStorage.Factories;
using FlowStorage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;
using Moq;
using FlowStorage.Abstractions.IBlobWrappers;
using FlowStorage.Abstractions.Factories;
using FlowStorage;
using Docker.DotNet.Models;

namespace FlowStorageTests.UnitTests.Tests.Factories
{
    public class FlowStorageFactoryTests
    {
        [Fact]
        public void Create_WithAzureBlobStorage_ReturnsIneMemoryFlowStorage()
        {
            // Arrange
            var type = StorageType.InMemoryStorage;
            var connectionString = "TestConnectionString";
            IServiceProvider serviceProvider = BuildServiceProvider();
            var factory = new FlowStorageFactory(serviceProvider, type, connectionString);

            // Act
            var storage = factory.Create();

            // Assert
            Assert.IsType<InMemoryFlowStorage>(storage);
        }

        [Fact]
        public void Create_WithAzureBlobStorage_ReturnsAzureBlobFlowStorage()
        {
            // Arrange
            var type = StorageType.AzureBlobStorage;
            var connectionString = "TestConnectionString";
            IServiceProvider serviceProvider = BuildServiceProvider();
            var factory = new FlowStorageFactory(serviceProvider, type, connectionString);

            // Act
            var storage = factory.Create();

            // Assert
            Assert.IsType<AzureBlobFlowStorage>(storage);
        }

        [Fact]
        public void Create_WithLocalStorage_ReturnsLocalFlowStorage()
        {
            // Arrange
            var type = StorageType.LocalStorage;
            var connectionString = "TestConnectionString";
            IServiceProvider serviceProvider = BuildServiceProvider();
            var factory = new FlowStorageFactory(serviceProvider, type, connectionString);

            // Act
            var storage = factory.Create();

            // Assert
            Assert.IsType<LocalFlowStorage>(storage);
        }

        private IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            var blobServiceClientFactoryMock = new Mock<IBlobServiceClientFactory>();
            var blobServiceClientWrapper = new Mock<IBlobServiceClientWrapper>();
            blobServiceClientFactoryMock
                .Setup(f => f.Create(It.IsAny<string>()))
                .Returns(blobServiceClientWrapper.Object);

            services.AddSingleton(blobServiceClientFactoryMock.Object);
            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddLogging();

            return services.BuildServiceProvider();
        }
    }
}
