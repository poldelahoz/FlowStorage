using FlowStorage.Factories;
using FlowStorage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;
using FlowStorage.Abstractions;
using Azure.Storage.Blobs;
using Moq;
using FlowStorage.Abstractions.IBlobWrappers;

namespace FlowStorageTests.UnitTests.Tests.Factories
{
    public class FlowStorageFactoryTests
    {
        [Fact]
        public void Create_WithAzureBlobStorage_ReturnsAzureBlobFlowStorage()
        {
            // Arrange
            var settings = new Dictionary<string, string?>
            {
                { "FlowStorage:type", "AzureBlobStorage" },
                { "FlowStorage:connectionString", "TestConnectionString" }
            };
            IConfiguration configuration = BuildConfiguration(settings);
            IServiceProvider serviceProvider = BuildServiceProvider();
            var factory = new FlowStorageFactory(serviceProvider, configuration);

            // Act
            var storage = factory.Create();

            // Assert
            Assert.IsType<AzureBlobFlowStorage>(storage);
        }

        [Fact]
        public void Create_WithLocalStorage_ReturnsLocalFlowStorage()
        {
            // Arrange
            var settings = new Dictionary<string, string?>
            {
                { "FlowStorage:type", "LocalStorage" },
                { "FlowStorage:connectionString", "TestConnectionString" }
            };
            IConfiguration configuration = BuildConfiguration(settings);
            IServiceProvider serviceProvider = BuildServiceProvider();
            var factory = new FlowStorageFactory(serviceProvider, configuration);

            // Act
            var storage = factory.Create();

            // Assert
            Assert.IsType<LocalFlowStorage>(storage);
        }

        [Fact]
        public void Create_WithUnsupportedStorageType_ThrowsNotSupportedException()
        {
            // Arrange
            var settings = new Dictionary<string, string?>
            {
                { "FlowStorage:type", "UnsupportedType" },
                { "FlowStorage:connectionString", "TestConnectionString" }
            };
            IConfiguration configuration = BuildConfiguration(settings);
            IServiceProvider serviceProvider = BuildServiceProvider();

            // Act & Assert: La excepción se lanza en el constructor, por lo que se debe envolver allí.
            var exception = Assert.Throws<NotSupportedException>(() =>
                new FlowStorageFactory(serviceProvider, configuration)
            );
            Assert.Contains("UnsupportedType", exception.Message);
        }

        [Fact]
        public void Constructor_MissingConnectionString_ThrowsArgumentNullException()
        {
            // Arrange: Se omite el connection string para simular el error
            var settings = new Dictionary<string, string?>
            {
                { "FlowStorage:type", "AzureBlobStorage" }
            };
            IConfiguration configuration = BuildConfiguration(settings);
            IServiceProvider serviceProvider = BuildServiceProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FlowStorageFactory(serviceProvider, configuration));
        }

        [Fact]
        public void Constructor_MissingStorageType_ThrowsArgumentNullException()
        {
            // Arrange: Se omite el tipo para simular el error
            var settings = new Dictionary<string, string?>
            {
                { "FlowStorage:connectionString", "TestConnectionString" }
            };
            IConfiguration configuration = BuildConfiguration(settings);
            IServiceProvider serviceProvider = BuildServiceProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FlowStorageFactory(serviceProvider, configuration));
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

        private IConfiguration BuildConfiguration(Dictionary<string, string?> settings)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }
    }
}
