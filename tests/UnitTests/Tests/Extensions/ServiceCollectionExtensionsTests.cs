using Azure.Storage.Blobs;
using FlowStorage;
using FlowStorage.Abstractions.Factories;
using FlowStorage.Abstractions.IBlobWrappers;
using FlowStorage.Abstractions.Services;
using FlowStorage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowStorageTests.UnitTests.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddFlowStorage_RegistersInMemoryStorageServices_WhenConfiguredAsInMemoryStorage()
        {
            // Arrange: Configuración para LocalStorage
            var configValues = new Dictionary<string, string?>
            {
                { "FlowStorage:type", "InMemoryStorage" },
                { "FlowStorage:connectionString", "TestBasePath" }
            };
            IConfiguration configuration = BuildConfiguration(configValues);
            var services = new ServiceCollection();
            services.AddSingleton(configuration);
            services.AddLogging();

            // Act: Registra los servicios de FlowStorage según la configuración
            services.AddFlowStorage(configuration);
            var provider = services.BuildServiceProvider();

            // Assert:
            // Se debe registrar el IFlowStorage como alias de ILocalFlowStorage.
            var flowStorage = provider.GetService<IFlowStorage>();
            Assert.NotNull(flowStorage);
            Assert.IsType<InMemoryFlowStorage>(flowStorage);

            // Se debe poder resolver ILocalFlowStorage.
            var inMemoryStorage = provider.GetService<IInMemoryFlowStorage>();
            Assert.NotNull(inMemoryStorage);

            // No se debe registrar ILocalFlowStorage.
            var localStorage = provider.GetService<ILocalFlowStorage>();
            Assert.Null(localStorage);

            // No se debe registrar IAzureBlobFlowStorage.
            var azureStorage = provider.GetService<IAzureBlobFlowStorage>();
            Assert.Null(azureStorage);
        }

        [Fact]
        public void AddFlowStorage_RegistersLocalStorageServices_WhenConfiguredAsLocalStorage()
        {
            // Arrange: Configuración para LocalStorage
            var configValues = new Dictionary<string, string?>
            {
                { "FlowStorage:type", "LocalStorage" },
                { "FlowStorage:connectionString", "TestBasePath" }
            };
            IConfiguration configuration = BuildConfiguration(configValues);
            var services = new ServiceCollection();
            services.AddSingleton(configuration);
            services.AddLogging();

            // Act: Registra los servicios de FlowStorage según la configuración
            services.AddFlowStorage(configuration);
            var provider = services.BuildServiceProvider();

            // Assert:
            // Se debe registrar el IFlowStorage como alias de ILocalFlowStorage.
            var flowStorage = provider.GetService<IFlowStorage>();
            Assert.NotNull(flowStorage);
            Assert.IsType<LocalFlowStorage>(flowStorage);

            // Se debe poder resolver ILocalFlowStorage.
            var localStorage = provider.GetService<ILocalFlowStorage>();
            Assert.NotNull(localStorage);

            // No se debe registrar IAzureBlobFlowStorage.
            var azureStorage = provider.GetService<IAzureBlobFlowStorage>();
            Assert.Null(azureStorage);

            // No se debe registrar IInMemoryFlowStorage.
            var inMemoryStorage = provider.GetService<IInMemoryFlowStorage>();
            Assert.Null(inMemoryStorage);
        }

        [Fact]
        public void AddFlowStorage_RegistersAzureBlobStorageServices_WhenConfiguredAsAzureBlobStorage()
        {
            // Arrange: Configuración para LocalStorage
            var configValues = new Dictionary<string, string?>
            {
                { "FlowStorage:type", "AzureBlobStorage" },
                { "FlowStorage:connectionString", "UseDevelopmentStorage=true" }
            };
            IConfiguration configuration = BuildConfiguration(configValues);
            var services = new ServiceCollection();
            services.AddSingleton(configuration); 
            services.AddLogging();

            // Para Azure, se usa un mock para IBlobServiceClientFactory.
            var blobServiceClientFactoryMock = new Mock<IBlobServiceClientFactory>();
            var blobServiceClientWrapper = new Mock<IBlobServiceClientWrapper>();
            blobServiceClientFactoryMock
                .Setup(f => f.Create(It.IsAny<string>()))
                .Returns(blobServiceClientWrapper.Object);
            services.AddSingleton(blobServiceClientFactoryMock.Object);

            // Act: Registra los servicios de FlowStorage según la configuración
            services.AddFlowStorage(configuration);
            var provider = services.BuildServiceProvider();

            // Assert:
            // IFlowStorage debe resolverse como AzureBlobFlowStorage.
            var flowStorage = provider.GetService<IFlowStorage>();
            Assert.NotNull(flowStorage);
            Assert.IsType<AzureBlobFlowStorage>(flowStorage);

            // Se debe poder resolver IAzureBlobFlowStorage.
            var azureStorage = provider.GetService<IAzureBlobFlowStorage>();
            Assert.NotNull(azureStorage);

            // No se debe registrar ILocalFlowStorage.
            var localStorage = provider.GetService<ILocalFlowStorage>();
            Assert.Null(localStorage);

            // No se debe registrar IInMemoryFlowStorage.
            var inMemoryStorage = provider.GetService<IInMemoryFlowStorage>();
            Assert.Null(inMemoryStorage);
        }

        [Fact]
        public void AddFlowStorage_ThrowsNotSupportedException_ForUnsupportedStorageType()
        {
            // Arrange: Configuración con un tipo no soportado.
            var configValues = new Dictionary<string, string?>
            {
                { "FlowStorage:type", "UnsupportedType" },
                { "FlowStorage:connectionString", "SomeConnectionString" }
            };
            IConfiguration configuration = BuildConfiguration(configValues);
            var services = new ServiceCollection();
            services.AddLogging();

            // Act & Assert: Se espera que se lance NotSupportedException al registrar los servicios.
            Assert.Throws<NotSupportedException>(() => services.AddFlowStorage(configuration));
        }

        private IConfiguration BuildConfiguration(Dictionary<string, string?> settings)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }
    }
}
